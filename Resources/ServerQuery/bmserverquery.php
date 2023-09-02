<?php
	
	/******************************************************************************\
		
		Bloodmasters Server Query Classes
		by Pascal vd Heiden (11/11/2006)
		
		Usage example:
		$sq = new ServerQuery("12.34.56.78", 6969);
		echo("My server title is: " . $sq->title);
		
	\******************************************************************************/
	
	// This returns current time in milliseconds
	function gettimems()
	{
		list($usec, $sec) = explode(" ",microtime());
		return ($sec . substr($usec, 2, 3));
	}
	
	
	// This parses and provides client information
	class ClientInfo
	{
		// Client information
		var $name = "";
		var $team = 0;
		var $spectator = 0;
		var $ping = 0;
		
		// Constructor: Parses client information from anser at the
		// given offset and adjusts offset.
		function ClientInfo($answer, &$offset)
		{
			// Read the name
			$len = unpack("C", substr($answer, $offset));
			$this->name = substr($answer, $offset + 1, $len[1]);
			$offset += 1 + $len[1];
			
			// Read the team and spectator
			$d = unpack("C2", substr($answer, $offset));
			$this->team = $d[1];
			if($d[2] != 0) $this->spectator = 1; else $this->spectator = 0;
			$offset += 2;
			
			// Read the ping
			$d = unpack("n", substr($answer, $offset));
			$this->ping = $d[1];
			$offset += 2;
		}
	}
	
	
	// This queries a server and provides the server information
	class ServerQuery
    {
		// Information variables
		var $title = "Unknown server";
		var $locked = 0;
		var $website = "";
		var $maxclients = 0;
		var $maxplayers = 0;
		var $gametype = 0;
		var $map = "";
		var $numclients = 0;
		var $numplayers = 0;
		var $protocol = 0;
		var $scorelimit = 0;
		var $timelimit = 0;
		var $joinsmallestteam = 0;
		var $buildversion = "Unknown server type or version";
		var $clients = array();
		
		// Constructor: Performs a query and parses the result
		function ServerQuery($address, $port, $timeout = 200)
		{
			// Open a UDP socket
			$sck = null;
			$sck = fsockopen("udp://" . $address, $port, $errstr, $errnum);
			
			// Check if opened
			if($sck == null)
			{
				// Unable to open socket
				$title = "Unable to open UDP socket for " . $address . ":" . $port;
				return;
			}
			
			// Send information request to server
			$data = pack("SCL", 7, 13, 0);
			if(fwrite($sck, $data) < 7)
			{
				// Unable to send data
				$title = "Unable to send UDP data to " . $address . ":" . $port;
				return;
			}
			
			// Dont block when waiting for response
			stream_set_blocking($sck, false);
			
			// Wait for response
			$starttime = gettimems();
			while($starttime + $timeout >= gettimems())
			{
				// Read all bytes
				$answer = fread($sck, 2048);
				
				// Read anything?
				if(strlen($answer) > 0)
				{
					//$codes = unpack("C" . strlen($answer), $answer);
					//print_r($codes);
					
					// Read the title
					$len = unpack("C", substr($answer, 7));
					$this->title = substr($answer, 8, $len[1]);
					$offset = 8 + $len[1];
					
					// Read locked
					$d = unpack("C", substr($answer, $offset));
					if($d[1] != 0) $this->locked = 1; else $this->locked = 0;
					$offset += 1;
					
					// Read the website
					$len = unpack("C", substr($answer, $offset));
					$this->website = substr($answer, $offset + 1, $len[1]);
					$offset += 1 + $len[1];
					
					// Read max clients and players
					$d = unpack("C2", substr($answer, $offset));
					$this->maxclients = $d[1];
					$this->maxplayers = $d[2];
					$offset += 2;
					
					// Read game type
					$d = unpack("C", substr($answer, $offset));
					$this->gametype = $d[1];
					$offset += 1;
					
					// Read the map name
					$len = unpack("C", substr($answer, $offset));
					$this->map = substr($answer, $offset + 1, $len[1]);
					$offset += 1 + $len[1];
					
					// Read current clients and players
					$d = unpack("C2", substr($answer, $offset));
					$this->numclients = $d[1];
					$this->numplayers = $d[2];
					$offset += 2;
					
					// Read protocol
					$d = unpack("C", substr($answer, $offset));
					$this->protocol = $d[1];
					$offset += 1;
					
					// Check if we should read any further
					if($this->protocol >= 27)
					{
						// Read scorelimit, timelimit
						$d = unpack("n2", substr($answer, $offset));
						$this->scorelimit = $d[1];
						$this->timelimit = $d[2];
						$offset += 4;
						
						// Read join smallest team
						$d = unpack("C", substr($answer, $offset));
						if($d[1] != 0) $this->joinsmallestteam = 1; else $this->joinsmallestteam = 0;
						$offset += 1;
						
						// Read all clients
						for($i = 0; $i < $this->numclients; $i++)
						{
							// Make client classes
							$this->clients[$i] = new ClientInfo($answer, $offset);
						}
					}
					
					// Check if we should read any further
					if($this->protocol >= 28)
					{
						// Read build description
						$len = unpack("C", substr($answer, $offset));
						$this->buildversion = substr($answer, $offset + 1, $len[1]);
						$offset += 1 + $len[1];
					}
					
					// Done
					break;
				}
			}
		}
		
		// This returns a string of all information
		function ToString()
		{
			// Server information
			$s = "Title: " . $this->title . "<br>" .
			     "Build: " . $this->buildversion . "<br>" .
				 "Website: " . $this->website . "<br>" .
			     "Locked: " . $this->locked . "<br>" .
				 "Max clients: " . $this->maxclients . "<br>" .
				 "Max players: " . $this->maxplayers . "<br>" .
				 "Game type: " . $this->gametype . "<br>" .
				 "Map: " . $this->map . "<br>" .
				 "Clients: " . $this->numclients . "<br>" .
				 "Players: " . $this->numplayers . "<br>" .
				 "Protocol: " . $this->protocol . "<br>" .
				 "Scorelimit: " . $this->scorelimit . "<br>" .
				 "Timelimit: " . $this->timelimit . "<br>" .
				 "Join Smallest: " . $this->joinsmallestteam . "<br>";
			
			// Spacing
			$s .= "<br>";
			
			// Go for all clients
			foreach($this->clients as $c)
			{
				// Client information
				$s .= "Player name: " . $c->name . "<br>" .
				      "Player team: " . $c->team . "<br>" .
					  "Player spectator: " . $c->spectator . "<br>" .
					  "Player ping: " . $c->ping . "<br>";
				
				// Spacing
				$s .= "<br>";
			}
			
			// Return string
			return $s;
		}
	}
	
?>
