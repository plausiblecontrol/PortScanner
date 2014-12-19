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
      int firstP;
      int lastP;
      bool debug = false;
      if (args.Length == 0) {
        showHelp();
      } else {
        try {
          if (args.Length > 3){
            debug = true;
            firstP = Convert.ToInt32(args[1]);
          lastP = Convert.ToInt32(args[2]) + 1;
          string[] hosts = args[0].Split(',');
          foreach (string host in hosts) { 
          Console.WriteLine("Scanning {0} from {1} to {2} for open ports...\nThis could take a few minutes.\n", host, firstP, lastP-1);
          Parallel.For(firstP, lastP, i => {
            Scan(i, host, debug);
          });
          }
          } else {
            if (args.Length == 2) {
              string[] hosts = args[0].Split(',');
              int[] ports = args[1].Split(',').Select(int.Parse).ToArray();

              foreach (string host in hosts) {
                Console.WriteLine("Scanning {0} for open ports..\n", host);
                Parallel.ForEach(ports, i => {
                  Scan(i, host, debug);
                });
              }
            } else {//args = 0 1 or 3
              int[] ports = args[1].Split(',').Select(int.Parse).ToArray();
              if(ports.Length>1){
                if (args.Length == 3)
                  debug = true;
                string[] hosts = args[0].Split(',');
                foreach (string host in hosts) {
                  Console.WriteLine("Scanning {0} for open ports..\n", host);
                  Parallel.ForEach(ports, i => {
                    Scan(i, host, debug);
                  });
                }
              } else {//ports = 1
                try {
                  lastP = Convert.ToInt32(args[2]) + 1;
                  debug = false;
                } catch {
                  debug = true;
                }
                if (debug) {
                  string[] hosts = args[0].Split(',');
                  foreach (string host in hosts) {
                    Scan(ports[0], host, debug);
                  }
                } else {
                  string[] hosts = args[0].Split(',');
                  firstP = Convert.ToInt32(args[1]);
                  lastP = Convert.ToInt32(args[2]) + 1;
                  foreach (string host in hosts) {
                    Console.WriteLine("Scanning {0} from {1} to {2} for open ports...\nThis could take a few minutes.\n", host, firstP, lastP - 1);
                    Parallel.For(firstP, lastP, i => {
                      Scan(i, host, debug);
                    });
                  }
                }
                
                
              }
            }
          }
        } catch {
          showHelp();
        }
      }
      Console.WriteLine("\nFinished.");
    }

    public static void showHelp() {
      Console.WriteLine("PartSconner.exe [host/ip,host2/ip2,..] [starting port] [ending port] (showall?)");
      Console.WriteLine("PartSconner.exe [host/ip,host2/ip2,..] [port,port2,..] (showall?)");
    }

    public static void Scan(int port, string txtIPaddress, bool allData) {
      TcpClient partS = new TcpClient();
      partS.SendTimeout = 10;
      partS.ReceiveTimeout = 10;
      try {
        partS.Connect(txtIPaddress, port);
        Console.WriteLine("Port " + port + " open!!!");
      } catch (Exception e){
        if (allData) {
          if (e.Message.Substring(0, 1) == "N") {
            Console.WriteLine("Port " + port + " was blocked.");
          } else {
            Console.WriteLine("Port " + port + " is closed.");
          }
        }
      }
      partS.Close();
    }
  }
}
