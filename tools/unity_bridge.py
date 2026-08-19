"""Direct client for MCP-for-Unity stdio bridge (framed TCP on port 6400).

Usage:
  python unity_bridge.py ping
  python unity_bridge.py '<json>'          (command JSON string)
  python unity_bridge.py --file cmd.json   (command JSON from file)

Protocol: server sends ASCII handshake line "WELCOME UNITY-MCP 1 FRAMING=1\n",
then framed messages: 8-byte big-endian length + UTF-8 payload.
"""
import json
import socket
import struct
import sys

HOST, PORT = "127.0.0.1", 6400
TIMEOUT = 120


def read_exact(sock, n):
    buf = b""
    while len(buf) < n:
        chunk = sock.recv(n - len(buf))
        if not chunk:
            raise IOError("connection closed")
        buf += chunk
    return buf


def main():
    if len(sys.argv) < 2:
        print("usage: unity_bridge.py <json|ping|--file path>", file=sys.stderr)
        sys.exit(2)

    if sys.argv[1] == "--file":
        with open(sys.argv[2], "r", encoding="utf-8") as f:
            payload = f.read()
    else:
        payload = sys.argv[1]

    if payload.strip() != "ping":
        json.loads(payload)  # validate

    sock = socket.create_connection((HOST, PORT), timeout=TIMEOUT)
    try:
        # handshake line
        line = b""
        while not line.endswith(b"\n"):
            line += sock.recv(1)
        data = payload.encode("utf-8")
        sock.sendall(struct.pack(">Q", len(data)) + data)
        (length,) = struct.unpack(">Q", read_exact(sock, 8))
        resp = read_exact(sock, length).decode("utf-8")
        print(resp)
    finally:
        sock.close()


if __name__ == "__main__":
    main()
