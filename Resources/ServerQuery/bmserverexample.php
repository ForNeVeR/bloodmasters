<?php
	
	// Include the required classes
	include_once("bmserverquery.php");
	
	// Query the server
	$q = new ServerQuery("217.67.231.10", 6969);
	
	// Output test info
	echo($q->ToString() . "<br>");
	
?>