<?php

if ($_SERVER['REQUEST_METHOD'] === 'POST')
{
    if (!isset($_POST['gifts']))
        $_POST['gifts'] = [];

    require_once(__DIR__ . '/recodex_lib.php');

    var_dump($_POST);
    $inv = invalidFields();
    if (empty($inv))
    {
        $uD = intval($_POST['unboxDay']);
        foreach(['fromTime', 'toTime'] as $v)
        {
            if(!empty($_POST[$v])) {
                $$v = (strtotime($_POST[$v]) - strtotime('00:00')) / 60;
            }
            else {
                $$v = null;
            }
        }

        $gC = in_array('other', $_POST['gifts']) ? $_POST['giftCustom'] : null;

        recodex_save_survey(
            $_POST['firstName'], $_POST['lastName'], $_POST['email'],
            $_POST['deliveryBoy'], $uD, $fromTime, $toTime, $_POST['gifts'], $gC
        );
    }
    else
    {
        recodex_survey_error("Invalid form values.", array_keys($inv));
    }

    header('Location: ' . $_SERVER['PATH_INFO'], true, 301);
    exit;
}
else
{
    require __DIR__ . '/form_template.html';
}

function invalidFields()
{
    $res = [];
    $required = [
        'firstName' => 'string',
        'lastName' => 'string',
        'fromTime' => '',
        'toTime' => '',
        'email' => 'email',
        'deliveryBoy' => 
            ['jesus', 'santa', 'moroz', 'hogfather', 'czpost', 'fedex'],
        'unboxDay' =>
            [24, 25]
    ];

    $max_len = [
        'firstName' => 100,
        'lastName' => 100,
        'email' => 200,
        'giftCustom' => 100,
        'fromTime' => 5,
        'toTime' => 5
    ];

    foreach($required as $k => $v)
    {
        if (!isset($_POST[$k]))
        {
            $res[$k] = TRUE;
            echo "Unset value: " . $k . "\n";
            continue;
        }
        if ($v === 'string' && empty($_POST[$k]))
        {
            echo "Empty value: " . $k . "\n";
            $res[$k] = TRUE;
            continue;
        }
        if ($v == 'email' && !filter_var($_POST[$k], FILTER_VALIDATE_EMAIL))
        {
            echo "Bad email: " . $k . "\n";
            $res[$k] = TRUE;
            continue;
        }
        if (is_array($v) && !in_array($_POST[$k], $v))
            $res[$k] = TRUE;
    }

    foreach($max_len as $k => $v)
    {
        if (isset($_POST[$k]) && strlen($_POST[$k]) > $v)
            $res[$k] = TRUE;
    }

    foreach($_POST['gifts'] as $v)
    {
        if (!in_array($v, ['socks', 'points', 'jarnik', 'cash', 'teddy', 'other']))
            $res['gifts'] = TRUE;
    }
    if (in_array('other', $_POST['gifts']) && empty($_POST['giftCustom']))
        $res['giftCustom'] = TRUE;

    foreach(['fromTime', 'toTime'] as $v)
    {
        if (!empty($_POST[$v]) && 
            (!preg_match('/^[0-9]{1,2}:[0-9]{2}$/', $_POST[$v]) || !strtotime($_POST[$v]))
            )
        {
            $res[$v] = TRUE;
            echo "Bad time format: " . $v . "\n";
        }
            
    }

    return $res;
}
