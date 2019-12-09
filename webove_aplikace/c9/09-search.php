<!DOCTYPE html>
<html>
<head>
    <title>Search</title>
</head>
<body>

<form method="GET" action="">
    <input type="text" name="search" />
    <input type="submit" value="Search" />
<form>

<h2>Search results:</h2>
<ul>
<?php

require __DIR__ . '/db.php';

$connection = new mysqli($db_config['server'], $db_config['login'], $db_config['password'], $db_config['database']);
if ($connection->connect_error) {
    die("Could not connect to the database");
}

/**
 * Search the items and return the result as array of strings...
 */
function searchItems($connection, $searchString)
{
    $query = $connection->prepare('SELECT name FROM items WHERE name LIKE CONCAT(?, "%")');
    $query->bind_param('s', $searchString);
    $query->execute();
    $res = [];
    if ($items = $query->get_result()) {
        while ($row = $items->fetch_assoc()) {
            $res[] = $row['name'];
        }
    }

    return $res;
}

if (isset($_GET['search'])) {
    $items = searchItems($connection, $_GET['search']);
    foreach ($items as $item) {
        echo "<li>", htmlspecialchars($item), "</li>\n";
    }
}

$connection->close(); // optional


?>
</ul>
</body>
</html>
