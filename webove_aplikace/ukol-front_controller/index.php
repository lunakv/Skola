<?php

try {
    showPage($_GET['page']);
} catch (Exception $e) {
    return error(500);
}

function showPage($page)
{
    if (!validPath($page))
        return error(400);

    $tmpPath = getRealPath(__DIR__ . '/templates/', $page);
    if ($tmpPath === FALSE)
        return error(404);

    $desc = getDescriptor($page);

    $params = transformParams($desc);
    if ($params === FALSE)
        return error(400);

    require __DIR__ . '/templates/_header.php';
    includePage($tmpPath, $params);
    require __DIR__ . '/templates/_footer.php';
}

function validPath($param)
{
    if (!preg_match('/^[a-zA-Z\/]+$/', $param)) return FALSE;
    return !preg_match('/^\/|\/\/|\/$/', $param);
    
}

function getRealPath($base, $param)
{
    $p = $base . $param . '.php';
    if (file_exists($p)) return $p;
    $p = $base . $param . '/index.php';
    if (file_exists($p)) return $p;
    return FALSE;
}

function getDescriptor($path)
{
    $fullPath = getRealPath(__DIR__ . '/parameters/', $path);
    if ($fullPath === FALSE) return [];

    return (include $fullPath);
}

function transformParams($desc)
{
    $res = [];
    foreach ($desc as $name => $type) {
        $val = $_GET[$name];
        if (!isset($val)) 
            return FALSE;
        if ($type === 'int' && !is_numeric($val))
            return FALSE;
        if (is_array($type) && !in_array($val, $type))
            return FALSE;
        
        $res[$name] = ($type === 'int') ? intval($val) : $val;
    }
    return $res;
}

function includePage($path, $params)
{
    foreach ($params as $k => $v)
    {
        $$k = $v;
    }

    require $path;
}

function error($code)
{
    switch ($code) {
        case 400:
            $msg = '400 Bad Request';
            break;
        case 404:
            $msg = '404 Not Found';
            break;
        case 500:
            $msg = '500 Internal Server Error';
            break;
        default:
            $msg = '';
    }

    echo '<p>'.$msg.'</p>';
    http_response_code($code);
}
