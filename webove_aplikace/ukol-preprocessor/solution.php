<?php

class ConfigPreprocessor
{
	private $config;
		
	public function __construct($config)
	{
		$this->config = $this->findTasks($config);
	}

	function findTasks($obj): array
	{
		$res = [];
		if ($this->isTask($obj))
		{
			$res[] = new TaskProxy($obj);
		}
		else if (is_array($obj))
		{
			foreach ($obj as $key => $value) {
				$res = array_merge($res, $this->findTasks($value));
			}
		}
		else if (is_object($obj))
		{
			foreach (get_object_vars($obj) as $key => $value) {
				$res = array_merge($res, $this->findTasks($value));
			}
		}

		return $res;
	}

	function isTask($t): bool
	{
		$properties = ['id', 'command', 'priority', 'dependencies'];
		foreach ($properties as $key => $value) {
			if (is_array($t) && !array_key_exists($value, $t))
				return FALSE;
			if (!is_array($t) && !isset($t->$value))
				return FALSE;
		}
		return TRUE;
	}

	/**
	 * Get an array of tasks from the config in the right order.
	 */
	public function getAllTasks()
	{
		$c = $this->config;
		$result = [];
		$resolved = [];

		while (!empty($c)) {
			$best = -1;
			foreach ($c as $k => $task) {
				if (!$task->isFree($resolved))
				{
					continue;	
				}
				if ($best == -1)
				{
					$best = $k;
				}
				else if ($task->getPriority() > $c[$best]->getPriority())
				{
					$best = $k;
				}
			}

			if ($best == -1)
				throw new Exception("Unresolvable dependency state reached.");
			$result[] = $c[$best];
			$resolved[$c[$best]->getId()] = TRUE;
			unset($c[$best]);
		}

		return array_map(function($m) {return $m->getTask();}, $result);
	}
}

class TaskProxy
{
	private $task;
	private $type;	// TRUE for array, FALSE for object

	public function __construct($task) 
	{
		$this->task = $task;
		$this->type = is_array($task);
		$this->deps = array();
	}

	function get($property)
	{
		return $this->type ? $this->task[$property] : $this->task->$property;
	}

	public function getId()
	{
		return $this->get('id');
	}

	public function getPriority()
	{
		return $this->get('priority');
	}

	public function isFree(&$resolved)
	{
		foreach ($this->get('dependencies') as $k => $dep)
		{
			if (!isset($resolved[$dep])) 
			{
				return FALSE;
			}
		}
		return TRUE;
	}

	public function getTask()
	{
		return $this->task;
	}
}



