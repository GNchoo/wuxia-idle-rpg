<?php
$DB_HOST = "localhost";
$DB_NAME = "your_database_name";
$DB_USER = "your_database_username";
$DB_PASS = "your_database_password";

header("Content-Type: application/json; charset=utf-8");

try {
    $pdo = new PDO(
        "mysql:host=$DB_HOST;dbname=$DB_NAME;charset=utf8mb4",
        $DB_USER,
        $DB_PASS,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(["success" => false, "error" => "db_connect_failed"]);
    exit;
}

function json_response($arr) {
    echo json_encode($arr);
    exit;
}