<?php

class Router
{
	public function dispatch()
	{
		$prefix = strtolower($_SERVER['REQUEST_METHOD']);
		if (empty($_GET['action'])) $this->error(400);
		
		$action = $this->getAction($_GET['action']);
		$file = __DIR__ . '/controllers/' . $action['dir'] . $action['ctr'] . '.php';
		$class = $action['ctr'] . 'Controller';
		$method = $prefix . $action['mtd'];

		if (!file_exists($file)) $this->error(404);
		require $file;
		if (!class_exists($class)) $this->error(404);
		$controller = new $class();
		if (!method_exists($controller, $method)) $this->error(404);
		
		$rm = new ReflectionMethod($controller, $method);
		$params = ($prefix === 'get') ? $_GET : $_POST;
		$args = $this->getArgs($rm->getParameters(), $params);
		try {
		$res = $rm->invokeArgs($controller, $args);
		} catch (Exception $e) {
			$this->error(500, $e->getMessage());
		}

		if ($res === null) $this->error(204);
		$res = json_encode($res);
		echo $res;
	}
	
	function getAction($action) 
	{
		if (preg_match('/^(?<dir>([a-zA-Z_]+\/)*)(?<ctr>[a-zA-Z_]+)\/(?<mtd>[a-zA-Z_]+)$/', $action, $matches)) {
			return $matches;
		} else {
			$this->error(400);
		}
	}

	function getArgs($rparams, $from)
	{
		$res = array();
		foreach ($rparams as $rp) {
			$name = $rp->getName();
			if (empty($from[$name])) $this->error(400);
			$res[] = $from[$name];
		}
		return $res;
	}

	function error($code, $msg = null)
	{
		http_response_code($code);
		if ($msg === null)
		{
			switch ($code) {
				case 204:
					$msg = 'No Content';
					break;
				case 400:
					$msg = 'Bad Request';
					break;
				case 404:
					$msg = 'Not Found';
					break;
				case 500:
					$msg = 'Internal Server Error';
					break;
				default:
					$msg = '';
					break;
			}
		}

		echo $msg;
		exit;
	}
}

