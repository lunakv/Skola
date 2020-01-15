<?php

require_once __DIR__ . '/database.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET')
{
    $db = new Database();
    base($db);
    $db->close();
}
else if ($_GET['action'] == 'add')
{
    $error = null;
    $db = new Database();
    try {
        addItem($db);
    } catch (Exception $e) {
        $error = $e->message;
        http_response_code($e->code ? $e->code : 500);
    }

    if ($error !== null) {
        base($db, $error);
        $db->close();
    } else {
        // 303 redirect to prevent resubmitting form
        $db->close();
        header("Location: " . parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH), true);
        http_response_code(303);
        exit;
    }
}
else
{
    try {
        api();
    } catch (Exception $e) {
        error(500, $e->getMessage());
    }
}

// JSON controller for asynchronous requests
function api()
{
    $db = new Database();
    $action = $_GET['action'];
    if ($action == 'delete')
    {
        $res = deleteItem($db);
    }
    else if ($action == 'edit')
    {
        $res = editItem($db);
    }
    else if ($action == 'swap')
    {
        $res = swapItem($db);
    }
    else 
    {
        error(400, "Invalid action");
    }
    header('Content-Type: application/json', true);
    echo json_encode($res);
    $db->close();
}

function addItem($db)
{
    if (!isset($_POST['name']))
        throw new Exception("No name specified.", 400);
    if (!isset($_POST['amount']))
        throw new Exception("No amount specified.", 400);
    if (!is_numeric($_POST['amount']))
        throw new Exception("Item amount is not a number.", 400);
    if ($_POST['amount'] <= 0)
        throw new Exception("Item amount must be positive", 400);
    
    $db->add($_POST['name'], $_POST['amount']);
	return [ "status" => "added", "name" => $_POST['name'], "amount" => $_POST['amount'] ];
}

function deleteItem($db)
{
    if (!isset($_POST['name']))
    {
        error(400, "No name specified");
    }
    
    $db->delete($_POST['name']);
    return [ "status" => "deleted", "name" => $_POST['name'] ];
}

function editItem($db)
{
    if (!isset($_POST['name']))
        error(400, "No name specified");
    if (!isset($_POST['amount']))
        error(400, "No amount specified");
    if (!is_numeric($_POST['amount']))
        error(400, "Item amount is not a number");
    if ($_POST['amount'] <= 0)
        error(400, "Item amount must be positive");
    
    $db->update($_POST['name'], $_POST['amount']);
    return [ "status" => "updated", "name" => $_POST['name'], "amount" => $_POST['amount'] ]; 
}

function swapItem($db) 
{
    if (!isset($_POST['name']))
        error(400, "No name specified");
    
    $newPos = $db->moveUp($_POST['name']);
    return [ "status" => "shifted", "name" => $_POST['name'], "position" => $newPos ];
}

// Loading the base site
function base($db, $error = null)
{
    loadTemplate('head', ['title' => 'Shopping List']);
    $prms = [];
    try {
        $prms['list'] = $db->getList();
        $prms['datalist'] = $db->getItems();
    } catch (Exception $e) {
        $error = $e->getMessage();
    }

    if (isset($error)) $prms['error'] = $error;
    loadTemplate('base', $prms);
    loadTemplate('foot');
}

// Generic template loading function
function loadTemplate($name, $vars = [])
{
    $file = __DIR__ . '/templates/' . $name . '.php';
    $vars = sanitizeHTML($vars);
    foreach ($vars as $k => $v) {
        $$k = $v;
    }
    require $file;
}

function sanitizeHTML($vars = []) {
    $res = [];
    foreach ($vars as $k => $v) {
        if (is_string($v))
            $res[$k] = htmlentities($v);
        elseif (is_array($v))
            $res[$k] = sanitizeHTML($v);
        else   
            $res[$k] = $v;
        
        if (is_object($v))
            foreach(get_object_vars($v) as $ok => $ov) {
                $res[$k]->$ok = sanitizeHTML($ov);
            }
    }
    return $res;
}

// Returns an appropriate error in JSON
function error($code, $msg = null)
{
    http_response_code($code);
    header('Content-Type: application/json', true);
	echo json_encode([ "status" => "error", "message" => $msg ]);
	exit;
}
