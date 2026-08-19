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

// We want: email, display_name, max_gold, max_gems, max_dps
$email        = isset($_POST['email']) ? trim($_POST['email']) : '';
$display_name = isset($_POST['display_name']) ? trim($_POST['display_name']) : '';
$max_gold     = isset($_POST['max_gold']) ? floatval($_POST['max_gold']) : 0;
$max_gems     = isset($_POST['max_gems']) ? floatval($_POST['max_gems']) : 0;
$max_dps      = isset($_POST['max_dps'])  ? floatval($_POST['max_dps'])  : 0;

if ($email === '' || $display_name === '') {
    http_response_code(400);
    echo json_encode(["error" => "bad_params"]);
    exit;
}

// Upsert to leaderboard
$stmt = $pdo->prepare("
    INSERT INTO leaderboard (user_email, display_name, max_gold, max_gems, max_dps)
    VALUES (:email, :name, :g, :gm, :d)
    ON DUPLICATE KEY UPDATE
        display_name = VALUES(display_name),
        max_gold = GREATEST(max_gold, VALUES(max_gold)),
        max_gems = GREATEST(max_gems, VALUES(max_gems)),
        max_dps  = GREATEST(max_dps,  VALUES(max_dps)),
        updated_at = CURRENT_TIMESTAMP
");

$stmt->execute([
    ':email' => $email,
    ':name'  => $display_name,
    ':g'     => $max_gold,
    ':gm'    => $max_gems,
    ':d'     => $max_dps
]);

echo json_encode(["status" => "ok"]);
