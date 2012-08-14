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
        table, th, tr, td { font-size: 8pt; }
    </style>
</head>
<body>
    Total Domains: <?= $redis->hlen("urn:link:domain-or-subdomain"); ?>
    <br />
    <br />
    Total Pages: <?= $redis->hlen("urn:link:data"); ?>
    <br />
    <br />
    Total Pages Collected as at: <br />
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
				echo round($persec,0)." pages/second";
				}
                echo "<br />";
            }
        ?>
    <br />
    URL Pool Count: <?= $redis->llen("urn:pool"); ?><br />
    
    
    <br /><br /><br /><br /><br />
    <form>
        Search domain name <input name="fSearch" type="text" size="50px" value="<?= $_REQUEST['fSearch'] ?>"/>
        <input name="fSubmit" type="submit" value="Search" />
        <br />
        <br />
        <br />
        <?php
            if (sizeof($links) > 0)
            {
                //$backlinks[] = array();
                foreach ($links as $link)
                {
                    $linkData = json_decode($redis->hget("urn:link:data", $link));
                    foreach ($linkData->Backlinks as $backlink)
                    {
                        $backlinks[$backlink][$link] = $link;
                    }
                }
                
                //var_dump($backlinks);
            }
        ?>

        <?php if (isset($backlinks)): ?>
        <h2>Backlinks</h2>
        <table border="1" width="100%">
            <tr>
                <th>Page URL</th>
                <th>IP</th>
                <th>Link Status</th>
                <th>Target URL</th>
                <th>Anchor Text</th>
                <th>OBL</th>
                <th>Total Links</th>
                <th>Last Crawl</th>
            </tr>
            <?php foreach ($backlinks as $backlink => $backlink_arr): ?>
                <?php $suppress = false; ?>
                <?php $backlink_obj = json_decode($redis->hget("urn:link:data", $backlink)); ?>
                <?php foreach ($backlink_arr as $link): ?>
                    <tr>
                        <td>
                            <a href="<?= $_SERVER['PHP_SELF'] ?>?fSearch=<?= $backlink_obj->DomainOrSubdomain ?>">
                                <?= ($suppress ? "&nbsp;" : $backlink); ?>
                            </a>
                        </td>
                        <td><?= ($suppress ? "&nbsp;" : join(", ", $backlink_obj->IPAddresses)); ?></td>
                        <td align="center"><?= ($suppress ? "&nbsp;" : 
                            json_decode($redis->hget("urn:link:status", $backlink))->StatusCode); ?></td>
                        <td><?= $link ?></td>
                        
                        <?php //anchor id = $link.***D***.$backlink ?>
                        <td>
                            <?php
                                $anchor_texts = json_decode(
                                    $redis->hget("urn:link:anchor", $link."***D***".$backlink));
                            ?>
                            <?php foreach ($anchor_texts as $anchor_text): ?>
                                <?php // $anchor->Text."; ".$anchor->Rel."; ".$anchor->Kind ?>
                                <?= htmlentities(json_decode($anchor_text)->Text); ?>
                                <br />
                            <?php endforeach; ?>
                        </td>
                        
                        <td align="right"><?= ($suppress ? "&nbsp;" : sizeof($backlink_obj->Backlinks)) ?></td>
                        <td align="right"><?= ($suppress ? "&nbsp;" : sizeof(json_decode($redis->hget("urn:link-child:data", $backlink)))) ?></td>
                        <td><?= $redis->hget("urn:link:data-last-date-crawl", $backlink) ?></td>
                    </tr>
                    <?php $suppress = true; ?>
                <?php endforeach; ?>
            <?php endforeach; ?>
        </table>
        <?php endif; ?>

        <?php if (isset($links)): ?>
        <h2>Pages</h2>
        <table border="1" width="100%">
            <tr>
                <th>Page URL</th>
                <th>External Links</th>
                <th>Referring Domains</th>
                <th>Referring IPs</th>
            </tr>
            <?php foreach ($links as $link): ?>
            <tr>
                <td><?= $link ?></td>
                <?php
                    $external_link_count = 0;
                    $childlinks_arr = json_decode($redis->hget("urn:link-child:data", $link));
                    $link_obj = json_decode($redis->hget("urn:link:data", $link));
                    if (isset($childlinks_arr))
                    {
                        foreach ($childlinks_arr as $childlink)
                        {
                            $childlink_obj = json_decode($redis->hget("urn:link:data", $childlink));
                            if ($childlink_obj->DomainOrSubdomain != $link_obj->DomainOrSubdomain)
                            {
                                ++$external_link_count;
                            }
                        }
                    }
                    
                    $referring_domain;
                    $referring_ip = 0;
                    foreach ($link_obj->Backlinks as $backlink)
                    {
                        $referring_domain[$backlink] = $backlink;
                        $referring_ip += sizeof(json_decode($redis->hget("urn:link:data", $backlink))->IPAddresses);
                    }
                    
                    
                ?>
                <td align="right"><?= $external_link_count; ?></td>
                <td align="right"><?= (isset($referring_domain) ? sizeof($referring_domain) : 0); ?></td>
                <td align="right"><?= $referring_ip; ?></td>
            </tr>
            <?php unset($referring_domain); ?>
            <?php endforeach; ?>
        </table>
        <?php endif; ?>


    </form>
</body>
</html>