using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace ConsoleApplication1
{

    class Program
    {
       
        static int porta = 9000;
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, porta);
            TcpClient clientSocket;
            Dictionary<string, handleClient> altriUtenti = new Dictionary<string, handleClient>(StringComparer.InvariantCultureIgnoreCase);
            int counter;
            string nome;
            serverSocket.Start();
            Console.WriteLine("Server chat Tognoli: " + porta);

            counter = -1;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                

                nome = "CLIENT" + Convert.ToString(counter);
                Console.WriteLine(nome + " avviato!");
                handleClient client = new handleClient();
               // altriUtenti.Add(client);
                
                client.startClient(clientSocket,nome,altriUtenti);
                

            }

            serverSocket.Stop();
            Console.WriteLine("Esco (in realtà non avviene mai, per come è scritto questo codice)");
            Console.ReadLine();
        }
    }

    //Classe che gestisce ogni client connesso separatamente
    public class handleClient
    {
        TcpClient clientSocket;
        string id;
        string nome="";
        char msgSwitch;
        Dictionary<string,handleClient> listaUtenti;
        public void inviaDati(string stringa)
        {
            NetworkStream stream= clientSocket.GetStream();
            Byte[] bytes_da_inviare;
            bytes_da_inviare = Encoding.ASCII.GetBytes(stringa);
            stream.Write(bytes_da_inviare, 0, bytes_da_inviare.Length);
        }

        public string leggiDati()
        {
            NetworkStream stream = clientSocket.GetStream();
            TcpClient cliente = clientSocket;
            Byte[] bytes = new Byte[cliente.ReceiveBufferSize];
            String stringa_ricevuta = "";
            int numero_bytes;
            do
            {
                numero_bytes = stream.Read(bytes, 0, cliente.ReceiveBufferSize);
                stringa_ricevuta += Encoding.ASCII.GetString(bytes, 0, numero_bytes);

            } while (numero_bytes >= cliente.ReceiveBufferSize);
            return stringa_ricevuta;
        }

        

        public void startClient(TcpClient inClientSocket, string id, Dictionary<string, handleClient> altriUtenti)
        {
            this.clientSocket = inClientSocket;
            
            this.id = id;
            this.listaUtenti = altriUtenti;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
            inviaDati(id);
        }
        private void doChat()
        {
          
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
            Console.WriteLine("La connessione è attiva all'indirizzo/porta: " + clientSocket.Client.RemoteEndPoint.ToString());
            Console.WriteLine("Utente: ["+id+"]");
           
            string dataFromClient = null;
            
            
            
            bool connesso = true;


            try
            {
                while (connesso)
                {


                    dataFromClient = leggiDati();

                    Console.WriteLine(DateTime.Now + " " + id + ": [" + dataFromClient + "]");

                    if (!dataFromClient.Equals("QUIT"))
                    {
                        string output;
                        msgSwitch = dataFromClient[0];
                        dataFromClient = dataFromClient.Substring(1);
                        switch (msgSwitch)
                        {
                            case 'N':
                                if (nome == "")
                                {

                                    output = "Y";
                                    if (dataFromClient.Contains("|") || dataFromClient.Contains(","))
                                        output = "N";
                                    else
                                    {
                                        foreach (KeyValuePair<string, handleClient> questo in listaUtenti)
                                        {
                                            if (questo.Value.nome.ToLower() == dataFromClient.ToLower())
                                            {
                                                output = "N";
                                                break;
                                            }

                                        }
                                        if (output == "Y")
                                        {
                                            this.nome = dataFromClient;
                                            listaUtenti[this.nome] = (this);
                                        }
                                    }
                                    inviaDati(output);
                                }
                                break;
                            case 'L':
                                if (nome != "")
                                {
                                    output = "U";
                                    foreach (KeyValuePair<string, handleClient> questo in listaUtenti)
                                    {
                                        output += questo.Value.nome + "|";
                                    }
                                    output = output.Substring(0, output.Length - 1);
                                    inviaDati(output);
                                }

                                break;
                            case 'M':
                                if (nome != "")
                                {
                                    string messaggio = dataFromClient.Split('|')[1];
                                    string[] destinatari;

                                    output = dataFromClient.Split('|')[0];
                                    if (output == "*")
                                    {
                                        output = "R";
                                        foreach (KeyValuePair<string, handleClient> questo in listaUtenti)
                                        {
                                            try
                                            {
                                                if (questo.Value.nome != this.nome)
                                                    questo.Value.inviaDati("M" + this.nome + "|" + messaggio);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Errore: " + ex.Message);
                                                output += questo.Value.nome + ",";
                                            }
                                        }


                                    }
                                    else
                                    {
                                        destinatari = output.Split(',');
                                        output = "R";

                                        for (int i = 0; i < destinatari.Length; i++)
                                        {
                                            try
                                            {
                                                listaUtenti[destinatari[i]].inviaDati("M" + this.nome + "|" + messaggio);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Errore: " + ex.Message);
                                                output += destinatari[i] + ",";
                                            }
                                        }

                                    }
                                    output = output.Substring(0, output.Length - 1);
                                    if (output == "")
                                    {
                                        output = "OK";
                                    }
                                    this.inviaDati(output);
                                }
                                break;

                        }

                    }
                    else
                    {
                        throw new Exception("[" + id + "] si è disconnesso in modo inaspettato.");
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Errore: "+ex.Message);
                Console.WriteLine("[" + id + "] si è disconnesso");
                connesso = false;
                listaUtenti.Remove(this.nome);
                
            }
            
        }
    }
}
