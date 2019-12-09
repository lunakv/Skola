<?php

/**
 * Object representing time without date or timezone (i.e., relative time or time of the day).
 * Required precision is 1 second.
 */
class Time
{
	private $hours;
	private $minutes;
	private $seconds;

	public function __construct(int $h, int $m, int $s = 0) {
		$this->setTime($h, $m, $s);
	}

	public function setTime(int $h, int $m, int $s = 0) {
		$this->hours = $h;
		$this->minutes = $m;
		$this->seconds = $s;
	}

	public function setSeconds($s) {
		$this->seconds = $s;
	}

	public function setMinutes($m) {
		$this->minutes = $m;
	}

	public function setHours($h) {
		$this->hours = $h;
	}

	public function getSeconds(): int
	{
		return $this->seconds;
	}

	public function getMinutes(): int
	{
		return $this->minutes;
	}

	public function getHours(): int
	{
		return $this->hours;
	}

	public function __toString(): string {
		if ($this->seconds == 0) 
			return sprintf("%d:%02d",$this->hours,$this->minutes);
		else
			return sprintf("%d:%02d:%02d",$this->hours,$this->minutes,$this->seconds);
	}

	public static function fromString($str) {
		if (preg_match('/^(?<hours>[0-9]{1,2}):(?<minutes>[0-9]{1,2})(:(?<seconds>[0-9]{1,2}))?/', $str, $matches)) {
			$seconds = isset($matches['seconds']) ? (int)$matches['seconds'] : 0;
			$minutes = (int)$matches['minutes'];
			$hours = (int)$matches['hours'];

			if ($hours > 23 || $minutes > 59 || $seconds > 59) return null;
			return new Time($hours, $minutes, $seconds);
		} else {
			return null;
		}
	}
}


/**
 * Function which takes associative array of events [time => event]
 * and produces a timetable array [hours][minutes][seconds] => event.
 * The input time (array key) is a string that should fit `Time::fromStr`.
 * The event is simply a string (keep it as is).
 */
function createTimetable(array $events): array
{
	$res = [];
	foreach ($events as $t => $e) {
		$time = Time::fromString($t);
		$res[$time->getHours()][$time->getMinutes()][$time->getSeconds()] = $e;
	}
	return $res;
}



// Testing data for the createTimetable function...
$testData = [
	'12:20' => 'Seminar begins',
	'13:50' => 'Seminar concludes',
	'14:00' => 'Lunch',
	'12:32:04' => 'Starting the assignment',
	'12:32:33' => 'Asking first questions',
	'12:22:42' => 'Sending the attendance form',
	'13:15' => 'First version implemented',
	'13:16' => 'First version not working -> debugging',
	'13:25' => 'Still debugging',
	'13:49' => 'Still yet debugging',
	'13:49:53' => 'Got it working somehow',
];

$timetable = createTimetable($testData);
var_dump($timetable);



/*
$x = Time::fromString("1:2:30");
$y = Time::fromString("12:30");
$z = Time::fromString("x:y:z");
$a = Time::fromString("");
$b = Time::fromString("3:420:00");
$c = Time::fromString(null);
$d = Time::fromString("2:25:xx");
echo $x . "\n";
echo $y . "\n";
echo $z . "\n";
echo $a . "\n";
echo $b . "\n";
echo $c . "\n";
echo $d . "\n";
*/