<?php

class ParrotController
{
	public function getEcho($message)
	{
		return [ "message" => $message ];
	}

	public function postEcho($message)
	{
		return [ "message" => $message, "method" => 'POST'];
	}


	public function getSilence()
	{
		return null;
	}
}
