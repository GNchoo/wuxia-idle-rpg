<?php
require_once "config.php";

$email = $_POST["email"] ?? "";
$password = $_POST["password"] ?? "";
$display_name = $_POST["display_name"] ?? "";

if (!$email || !$password || !$display_name) {
    json_response(["success" => false, "error" => "missing_fields"]);
}

if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    json_response(["success" => false, "error" => "invalid_email"]);
}

if (strlen($password) < 6) {
    json_response(["success" => false, "error" => "password_too_short"]);
}

try {
    $stmt = $pdo->prepare("SELECT id FROM users WHERE email = ?");
    $stmt->execute([$email]);
    if ($stmt->fetch()) {
        json_response(["success" => false, "error" => "email_exists"]);
    }

    $hash = password_hash($password, PASSWORD_BCRYPT);

    $stmt = $pdo->prepare("INSERT INTO users (email, password_hash, display_name) VALUES (?, ?, ?)");
    $stmt->execute([$email, $hash, $display_name]);

    $user_id = $pdo->lastInsertId();

    json_response([
        "success" => true,
        "user_id" => (int)$user_id,
        "display_name" => $display_name
    ]);
} catch (Exception $e) {
    json_response(["success" => false, "error" => "server_error"]);
}