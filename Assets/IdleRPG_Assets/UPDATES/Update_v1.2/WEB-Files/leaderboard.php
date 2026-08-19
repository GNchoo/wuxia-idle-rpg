<?php
// The same credentials as in config.php
$db_host = "localhost";
$db_name = "your_database_name";
$db_user = "your_database_username";
$db_pass = "your_database_password";

header('Content-Type: application/json; charset=utf-8');

try {
    $pdo = new PDO(
        "mysql:host=$db_host;dbname=$db_name;charset=utf8mb4",
        $db_user,
        $db_pass,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(["error" => "db"]);
    exit;
}

$stmt = $pdo->query("
    SELECT display_name, max_gold, max_gems, max_dps
    FROM leaderboard
    ORDER BY max_gold DESC
    LIMIT 100
");

$rows = $stmt->fetchAll(PDO::FETCH_ASSOC);
echo json_encode($rows);
