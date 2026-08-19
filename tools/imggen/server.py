"""로컬 이미지 생성 서버 — Z-Image-Turbo (Apache 2.0, Tongyi-MAI).

Grok API 대체: 같은 프롬프트를 받아 PNG를 돌려준다. Unity의 XaiIconGenerator가
LOCAL_IMG_URL(기본 http://127.0.0.1:8009)로 POST하면 이후 파이프라인(크로마키
제거 → 시트 분할 → 설치)은 그대로다.

    POST /generate  {"prompt": "...", "width": 1280, "height": 720, "seed": 123}
    → 200 image/png

기동:  .venv/Scripts/python.exe server.py   (첫 실행 시 모델 자동 다운로드 ~19GB)
모델 캐시는 이 폴더의 hf/ (H 드라이브) — C 드라이브를 채우지 않는다.
"""
import io
import json
import os
import sys

os.environ.setdefault("HF_HOME", os.path.join(os.path.dirname(os.path.abspath(__file__)), "hf"))

import torch
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

MODEL_ID = "Tongyi-MAI/Z-Image-Turbo"
PORT = 8009

print(f"[imggen] loading {MODEL_ID} (bf16, cuda) ...", flush=True)

def load_pipe():
    from diffusers import DiffusionPipeline
    try:
        from diffusers import ZImagePipeline  # diffusers가 정식 지원하면 이쪽
        pipe = ZImagePipeline.from_pretrained(MODEL_ID, torch_dtype=torch.bfloat16)
    except ImportError:
        pipe = DiffusionPipeline.from_pretrained(
            MODEL_ID, torch_dtype=torch.bfloat16, trust_remote_code=True)
    # 전체 상주(DiT+텍스트인코더+VAE)는 16GB를 넘어 공유 메모리로 넘친다
    # (실측: 스텝당 216초). 사용 중인 컴포넌트만 GPU에 올리면 DiT 단독 12GB로 여유.
    pipe.enable_model_cpu_offload()
    # 2K급 이미지의 VAE 디코드가 통짜로는 메모리를 넘겨 세그폴트(실측) —
    # 타일/슬라이스 디코드로 쪼갠다 (지원 안 하는 파이프라인이면 조용히 넘어감)
    for fn in ("enable_vae_slicing", "enable_vae_tiling"):
        try:
            getattr(pipe, fn)()
        except Exception:
            pass
    return pipe

PIPE = load_pipe()
print("[imggen] ready on port", PORT, flush=True)


class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        if self.path != "/generate":
            self.send_error(404)
            return
        try:
            n = int(self.headers.get("Content-Length", 0))
            req = json.loads(self.rfile.read(n).decode("utf-8"))
            prompt = req["prompt"]
            w = int(req.get("width", 1280))
            h = int(req.get("height", 720))
            steps = int(req.get("steps", 9))
            seed = req.get("seed")
            gen = torch.Generator("cuda").manual_seed(int(seed)) if seed is not None else None
            img = PIPE(prompt=prompt, width=w, height=h,
                       num_inference_steps=steps, guidance_scale=1.0,
                       generator=gen).images[0]
            buf = io.BytesIO()
            img.save(buf, format="PNG")
            data = buf.getvalue()
            self.send_response(200)
            self.send_header("Content-Type", "image/png")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)
            print(f"[imggen] ok {w}x{h} steps={steps} :: {prompt[:60]}...", flush=True)
        except Exception as e:  # 어떤 예외든 500 + 사유 — 클라이언트가 로그로 본다
            msg = f"{type(e).__name__}: {e}".encode("utf-8")
            self.send_response(500)
            self.send_header("Content-Type", "text/plain; charset=utf-8")
            self.send_header("Content-Length", str(len(msg)))
            self.end_headers()
            self.wfile.write(msg)
            print("[imggen] FAIL", msg.decode("utf-8"), file=sys.stderr, flush=True)

    def do_GET(self):
        # 헬스체크
        body = b"ok"
        self.send_response(200)
        self.send_header("Content-Length", "2")
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, fmt, *args):
        pass  # 기본 액세스 로그 소음 제거


if __name__ == "__main__":
    ThreadingHTTPServer(("127.0.0.1", PORT), Handler).serve_forever()
