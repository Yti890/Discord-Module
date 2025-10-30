<h1 align="center">Fix Discord Network</h1>
Knowing that there are issues with connecting the Bot to the Internet in some countries, for example in Russia, I have gathered resources that will help you bypass these 'restrictions'.

And remember that I do not take responsibility for what is said here and what you will do with it; you are doing it at your own risk.

Maybe i fix that, but i'm not sure.

<h2 align="center">Network DNS ( Linux )</h2>

## You may need to configure the DNS to get it to work, but this is not mandatory. However, if the workaround does not work, I recommend doing as written.

<h3>resolvconf</h3>

```bash
sudo apt install resolvconf  # Debian/Ubuntu
```
```bash
sudo yum install resolvconf  # CentOS/RHEL
```
( this is Cloundflare WARP )
```bash
echo "nameserver 1.1.1.1" | sudo tee /etc/resolvconf/resolv.conf.d/head
echo "nameserver 1.0.0.1" | sudo tee -a /etc/resolvconf/resolv.conf.d/head
echo "options use-vc" | sudo tee -a /etc/resolvconf/resolv.conf.d/head
sudo resolvconf -u
```
( this is Google DNS )
```bash
echo "nameserver 8.8.8.8" | sudo tee /etc/resolvconf/resolv.conf.d/head
echo "nameserver 8.8.4.4" | sudo tee -a /etc/resolvconf/resolv.conf.d/head
echo "options use-vc" | sudo tee -a /etc/resolvconf/resolv.conf.d/head
sudo resolvconf -u
```

<h2 align="center">Network DNS ( Windows )</h2>

## You may need to configure the DNS to get it to work, but this is not mandatory. However, if the workaround does not work, I recommend doing as written.

# You need use the Windows PowerShell ( Admin )

( For Windows 10 Server )
```bash
Get-DnsClient | Where-Object {$_.InterfaceAlias -like "*Ethernet*"} | Set-DnsClientServerAddress -ServerAddresses ("8.8.8.8","8.8.4.4")
New-Item -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DohInterfaceSettings" -Force

New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DohInterfaceSettings" `
  -Name "8.8.8.8" -PropertyType String -Value "https://dns.google/dns-query" -Force

New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DohInterfaceSettings" `
  -Name "8.8.4.4" -PropertyType String -Value "https://dns.google/dns-query" -Force
Get-DnsClientServerAddress
```
( For Windows 11 Server )
```bash
Set-DnsClientServerAddress -InterfaceAlias "Ethernet" -ServerAddresses 8.8.8.8, 8.8.4.4
Set-DnsClientDohServerAddress -ServerAddress 8.8.8.8 -DohTemplate "https://dns.google/dns-query" -AllowFallbackToUdp $false
Set-DnsClientDohServerAddress -ServerAddress 8.8.4.4 -DohTemplate "https://dns.google/dns-query" -AllowFallbackToUdp $false
```

<h2 align="center">Zapret ( Linux )</h2>

## In fact, there are many, but I will write down 2 here, they have instructions, read them, I have used both myself, so they work.

- [Zapret DPI](https://github.com/bol-van/zapret?tab=readme-ov-file)
- [Zapret Installer](https://github.com/Snowy-Fluffy/zapret.installer) 

<h2 align="center">Zapret ( Windows )</h2>

## Well... There can be a lot here, to be honest, you might even know it, but I'll leave the links anyway.

- [Zapret DPI](https://github.com/bol-van/zapret?tab=readme-ov-file)
- [zapret-discord-youtube](https://github.com/Flowseal/zapret-discord-youtube) 
