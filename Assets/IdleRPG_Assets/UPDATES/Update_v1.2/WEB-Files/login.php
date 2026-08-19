<?php
require_once "config.php";

$email = $_POST["email"] ?? "";
$password = $_POST["password"] ?? "";

if (!$email || !$password) {
    json_response(["success" => false, "error" => "missing_fields"]);
}

try {
    $stmt = $pdo->prepare("SELECT id, password_hash, display_name FROM users WHERE email = ?");
    $stmt->execute([$email]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);

    if (!$row || !password_verify($password, $row["password_hash"])) {
        json_response(["success" => false, "error" => "invalid_credentials"]);
    }

    $stmt = $pdo->prepare("UPDATE users SET last_login_at = NOW() WHERE id = ?");
    $stmt->execute([$row["id"]]);

    json_response([
        "success" => true,
        "user_id" => (int)$row["id"],
        "display_name" => $row["display_name"]
    ]);
} catch (Exception $e) {
    json_response(["success" => false, "error" => "server_error"]);
}