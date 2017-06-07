<?php
header("Cache-Control: no-store, no-cache, must-revalidate, max-age=0");
header("Cache-Control: post-check=0, pre-check=0", false);
header("Pragma: no-cache");

//db connect
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "imdb_test";
$conn = new mysqli($servername, $username, $password, $dbname);
if ($conn->connect_error)
{
    die("Connection failed: " . $conn->connect_error);
}

//add the names of all db films in the array
$arrayFilmID = array();
$sql = "SELECT gid FROM imdb_test.films";
$films = $conn->query($sql);
echo "Number of films in db: ".$films->num_rows."<br>";
if ($films->num_rows > 0)
{
	while ($row = $films->fetch_row())
	{
		$arrayFilmID[count($arrayFilmID)] = $row[0];
    }
}
else {echo "0 results";}

//add stills' ID in the array
$arrayStills = array();
$sql = "SELECT ID, gid, url FROM imdb_test.stills";
$still = $conn->query($sql);
echo "Number of stills: ".$still->num_rows."<br>";
if ($still->num_rows > 0)
{
	while ($row = $still->fetch_row())
	{
		$arrayStills[count($arrayStills)] = $row[0];
    }
}
else {echo "0 results";}

//pick a random still's id
//(seed with microseconds)
function make_seed()
{
  list($usec, $sec) = explode(' ', microtime());
  return $sec + $usec * 1000000;
}
srand(make_seed());

$rstill2 = rand(1, $still->num_rows);
echo "Random still's ID v0: ".$rstill2."<br>";

//$rsarray = array_rand($arrayStills, 1);
//$rstill = $arrayStills[$rsarray];

//find url and show image
$sqlRandomURL = "SELECT url, gid FROM imdb_test.stills WHERE ID='".$rstill2."' LIMIT 1";
$stillRandomURL = $conn->query($sqlRandomURL);
$row = $stillRandomURL->fetch_row();
echo "<img style='width:35%;' src='".$row[0]."'/><br>";                         //debug --img

//poll title
echo "<br><strong>Which film is this image from?</strong><br>";

//find out which movie this image is from
$sqlStillName = "SELECT title FROM imdb_test.films WHERE gid='".$row[1]."' LIMIT 1";
$stillName = $conn->query($sqlStillName);
$sname = $stillName->fetch_row();
echo $sname[0]." (".$row[1].")<br>";

//pick random movies for the poll
$rfarray = array_rand($arrayFilmID, 4);
foreach ($rfarray as $rFilm)
{
	$sqlFilmName = "SELECT title FROM imdb_test.films WHERE gid='".$arrayFilmID[$rFilm]."' LIMIT 1";
	$filmName = $conn->query($sqlFilmName);
	$row = $filmName->fetch_row();
	echo $row[0]." (".$arrayFilmID[$rFilm].")<br>";
}

$conn->close();
$arrayFilmID=array();
$arrayStills=array();

?>