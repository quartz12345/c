<?php
    require_once __DIR__.'/predis/autoload.php';
    Predis\Autoloader::register();

    //$redis = new Predis\Client(array("host" => "65.111.173.138"));
    //$redis = new Predis\Client(array("host" => "50.62.1.71"));
    $redis = new Predis\Client(array("host" => "127.0.0.1"));
    $links = array();
    if (isset($_REQUEST["fSearch"]))
    {
        $links = json_decode($redis->hget("urn:link:domain-or-subdomain", $_REQUEST["fSearch"]));
    }
    else
    {
        $_REQUEST['fSearch'] = 'www.articleteller.com';
    }
	$tz = new DateTimeZone( 'America/New_York');
	$now = new DateTime( 'now', $tz); // DateTime object corellated to user's timezone
	$localtime = $now->format('Y-m-d H:i:s');
	$localtime = strtotime($localtime);
?>
<html>
<head>
    <style type="text/css">
        html { font-family: Arial; font-size: 9pt; }
    </style>
</head>
<body>
    Total Domains: <?= $redis->hlen("urn:link:domain-or-subdomain"); ?>
    <br />
    <br />
    Total Links: <?= $redis->hlen("urn:link:data"); ?>
    <br />
    <br />
    Total Links Crawled as at: <br />
        <?php
            for ($i = 0; $i < 10; $i++)
            {
                $d = $localtime + (-$i * 24 * 60 * 60);
                echo date("n/j/Y", $d);
                echo ": ";
                $sizeof = sizeof(json_decode($redis->hget("urn:link:date-last-crawl", date("n/j/Y 12:00:00 \A\M", $d))));
				echo $sizeof;
				if($sizeof != 0){
				$persec = (($sizeof/60)/60);
				echo " -- ";
				echo round($persec,0)." links or pages/second";
				}
                echo "<br />";
            }
        ?>
    <br />
    URL Pool Count: <?= $redis->llen("urn:pool"); ?><br />
    
    
    <br /><br /><br /><br /><br />
    <form>
        Search domain name <input name="fSearch" type="text" value="<?= $_REQUEST['fSearch'] ?>"/>
        <input name="fSubmit" type="submit" value="Search" />
        <br />
        <br />
        <br />
        <?php
            if (sizeof($links) > 0)
            {
                foreach ($links as $link)
                {
                    echo $link . "<br/>";
                    $linkData = json_decode($redis->hget("urn:link:data", $link));
                    
                    echo "Backlinks: <ul>";
                    foreach ($linkData->Backlinks as $backlink)
                    {
                        echo "<li>" . $backlink . "</li>";
                    }
                    echo "</ul>";
                }
            }
        ?>
    </form>
</body>
</html>