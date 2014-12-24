using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace PartSconner {
  class Program {
    static void Main(string[] args) {
      string hosts = "";
      string ports = "";
      bool debug = false;
      int time = 0;
      int offset = 0;

      if (args.Length == 0) {
        showHelp();
        Environment.Exit(0);
      }

      try {//host and time
        time = Math.Abs(Convert.ToInt32(args[0]));
        offset = 1;
        hosts = args[1];
      } catch {
        if(args.Length>0)
          hosts = args[0];
      }
      try {//ports
        ports = args[1+offset];
      } catch { }

      try {//debug
        if (args[2 + offset] != null)
          debug = true;
      } catch { }

      string[] portList = procPorts(ports);
      string[] hostList = hosts.Split(',');

      if (args.Length == 0) {
        showHelp();
      } else {
        try {
          int[] myPorts = portList.Select(int.Parse).ToArray();
          if (time == 0) {
            sconner(hostList, myPorts, debug);
          } else {
            string[] tArgs = args.Skip(1).ToArray();
            while (true) {
              sconner(hostList, myPorts, debug);
              System.Threading.Thread.Sleep(time * 1000);
            }
          }
        } catch {
          Console.WriteLine("\nBad entry, see usage:\n");
          showHelp();
        } 
      }
      Console.WriteLine("\nFinished.");
    }

    public static string[] procPorts(string ports) {
      List<string> p = new List<string>();
      string[] procSplit = ports.Split(',');
      foreach (string ss in procSplit) {
        string[] dashes = ss.Split('-');
        if (dashes.Length == 2) {
          for (int i = Convert.ToInt32(dashes[0]); i <= Convert.ToInt32(dashes[1]); i++) {
            p.Add(i.ToString());
          }
        } else {
          p.Add(dashes[0]);
        }
      }
      return p.ToArray();
    }

    public static void sconner(string[] hosts, int[] ports, bool debug) {
      try {
        foreach (string host in hosts) {
          if (debug) {
            Console.WriteLine("\nScanning {0}'s ports..", host);
          } else {
            Console.WriteLine("\nScanning {0} for open ports only..", host);
          }          
          Parallel.ForEach(ports, i => {
            Scan(i, host, debug);
          });
        }
      } catch {
        Console.WriteLine("There was bad data in your parameters.");
      }
    }

    public static void showHelp() {
      Console.WriteLine("PartSconner.exe (loop time in s) [host/ip,host2/ip2,..] [port,port2,port3-port8,port9..] (showall?)");
    }

    public static void Scan(int port, string txtIPaddress, bool allData) {
      TcpClient partS = new TcpClient();
      partS.SendTimeout = 2000;
      partS.ReceiveTimeout = 2000;
      IAsyncResult result = partS.BeginConnect(txtIPaddress, port, null, null);
      //timeouts take 10-12 seconds, this forces that timeout to stop after 2000ms
      bool success = result.AsyncWaitHandle.WaitOne(2000, true);
      if (success && partS.Connected) {
        //didn't timeout and got connected - Open
        Console.WriteLine(txtIPaddress + ":" + port + " open!!!");
      } else if (success && allData && !partS.Connected) {
        //didn't timeout but couldn't connect, and want to show non-open - Blocked
        Console.WriteLine(txtIPaddress + ":" + port + " blocked.");
      } else if (!success && allData) {
        //timed out response and want to show non-open - Timed out
        Console.WriteLine(txtIPaddress + ":" + port + " timed out.");
      }
      partS.Close();
    }
  }
}
