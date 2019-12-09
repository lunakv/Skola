<?php

define('SEXES', [
	'M' => 'Male',
	'F' => 'Female',
	'O' => 'Other',
]);

/**
 * Internal function that saves the data to a JSON file.
 */
function saveData(string $fullName, int $age, string $sex, array $children)
{
	file_put_contents(__DIR__ . '/data/form.json', json_encode([
		'fullName' => $fullName,
		'age' => $age,
		'sex' => SEXES[$sex],
		'children' => $children,
	], JSON_PRETTY_PRINT));
}


require __DIR__ . '/templates/header.php';
require __DIR__ . '/templates/form.php';
require __DIR__ . '/templates/footer.php';
