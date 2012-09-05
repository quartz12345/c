<?php
    error_reporting(~0);
    
    $m = new Mongo();
    $db = $m->ls;
    
    $links = array();
    if (!isset($_REQUEST["fSearch"]))
    {
        $_REQUEST['fSearch'] = 'www.articleteller.com';
    }
    
    //$tz = new DateTimeZone( 'America/New_York');
    $tz = date_timezone_get(date_create(null, timezone_open("Australia/Sydney")));
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
    Total Domains: <?= $db->selectCollection("urn:domain:data")->count(); ?><br />
    Total Pages: <?= $db->selectCollection("urn:link:data")->count(); ?><br />
    Total Pages Collected as at: <br />
        <?php
            for ($i = 0; $i < 10; $i++)
            {
                $d = $localtime + (-$i * 24 * 60 * 60);
                echo date("ymd", $d);
                echo ": ";
                $result = $db->selectCollection("urn:crawldate:link")->findOne(array("_id" => date("ymd", $d)));
                $sizeof = sizeof($result['Value']);
                echo $sizeof . " pages/day";
                if($sizeof != 0)
                {
                    $persec = ((($sizeof / 24) / 60) / 60);             // ?
                    echo " (" . round($persec,0) . " pages/sec)";
                }
                echo "<br />";
            }
        ?>
    <br />
    <?php 
        $result = $db->selectCollection("urn:pool")->findOne();
    ?>
    URL Pool Count: <?= sizeof($result['Value']); ?><br />
    
    
    <br /><br /><br /><br /><br />
    <form>
        Search domain name <input name="fSearch" type="text" size="50px" value="<?= $_REQUEST['fSearch'] ?>"/>
        <input name="fSubmit" type="submit" value="Search" />
        <br />
        CRC32: <?= crc32_s($_REQUEST["fSearch"]); ?>
        <br />
        <br />
        <br />
        <?php
            $domainCollection = $db->selectCollection("urn:domain:data")->findOne(array("_id" => (string)crc32_s($_REQUEST["fSearch"])));
            $domainData = $domainCollection["Value"];
            if (isset($domainData))
            {
                if (sizeof($domainData["Links"]) > 0)
                {
                    foreach ($domainData["Links"] as $link)
                    {
                        $linkData = $db->selectCollection("urn:link:data")->findOne(array("_id" => $link));
                        foreach ($linkData["BackLinks"] as $backlink)
                        {
                            if ($linkData["Domain"] == $domainData["Domain"])
                            {
                                $backlinkData = $db->selectCollection("urn:link:data")->findOne(array("_id" => (string)crc32_s($backlink)));
                                $backlinks[$backlink][$linkData["Link"]] = $linkData["Link"];
                            }
                        }
                    }
                }
            }
        ?>

        <?php if (isset($domainData)): ?>
        <h2>Backlinks</h2>
        <table border="1" width="100%">
            <tr>
                <th>Back Link</th>
                <th>IP</th>
                <th>Link Status</th>
                <th>Target Page</th>
                <th width="100px">Anchor Text</th>
                <th>OBL</th>
                <th>Total Links</th>
                <th>Last Crawl</th>
            </tr>
            <?php foreach ($backlinks as $backlink => $links): ?>
                <?php $suppress = false; ?>
                <?php $backlinkData = $db->selectCollection("urn:link:data")->findOne(array("_id" => crc32_s($backlink))); ?>
                <?php $linkstatusCollection = $db->selectCollection("urn:link:status:current")->findOne(array("_id" => crc32_s($backlink))); ?>
                <tr>
                    <td>
                        <a href="<?= $_SERVER['PHP_SELF'] ?>?fSearch=<?= $backlinkData["Domain"] ?>">
                            <?= ($suppress ? "&nbsp;" : $backlink); ?>
                        </a>
                        CRC32: <?= crc32_s($backlink); ?>
                    </td>
                    <td><?= ($suppress ? "&nbsp;" : $backlinkData["IP"]); ?></td>
                    <td align="center"><?= ($suppress ? "&nbsp;" : $linkstatusCollection["Value"]); ?></td>
                    <td colspan="2">
                        <table border="1" width="100%">
                            <?php foreach ($links as $link): ?>
                                <tr>
                                    <td><?= $link ?></td>
                                    <?php
                                        $anchorCollection = $db->selectCollection("urn:anchor:data")->findOne(
                                            array("_id" => crc32_s($backlink . $link)));
                                        $anchorData = $anchorCollection["Value"];
                                    ?>
                                    <td width="100px"><?= $anchorData[0]["Text"]; ?></td>
                                </tr>
                            <?php endforeach; ?>
                        </table>
                    </td>
                </tr>
                <?php $suppress = true; ?>
            <?php endforeach; ?>
        </table>
        <?php endif; ?>

        <?php if (isset($domainData)): ?>
        <h2>Pages</h2>
        <table border="1" width="100%">
            <tr>
                <th>Page URL</th>
                <th>External Links</th>
                <th>Referring Domains</th>
                <th>Referring IPs</th>
            </tr>
        </table>
        <?php endif; ?>


    </form>
</body>
</html>
<?php
    // helper functions
    
    // Use sprintf to return unsigned values even if PHP is running in 32 bit mode
    // 64bit OSes usually returns unsigned values
    // ref: http://php.net/manual/en/function.crc32.php
    function crc32_s($value)
    {
        return sprintf("%u", crc32($value));
    }
?>