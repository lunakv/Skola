<?php

/*
 * Create an array primes from 2 to $max.
 */
function sieve($max)
{
	$prime = [];
	for ($i = 2; $i <= $max; $i++) {
		if (!isset($prime[$i])) {
			$prime[$i] = TRUE;
			for ($j = 2*$i; $j <= $max; $j += $i) {
				$prime[$j] = FALSE;
			}
		}
	}

	$res = [];
	foreach ($prime as $k => $v) {
		if ($v) $res[] = $k;
	}
	
	return $res;
}
