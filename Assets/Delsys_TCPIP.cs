using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
public class Delsys_TCPIP : MonoBehaviour
{
    //TCP/IP Parameters
    public static TcpClient commandSocket;
    public static NetworkStream commandStream;
    public static StreamReader commandReader;
    public static StreamWriter commandWriter;
    public static string response;
    public static bool connected;
    //Constructor
    public Delsys_TCPIP()
    //public Check_Connection(int n_imu, int n_muscles, string id, string sess)
    {
    }
    //
    public void Main()
    {
        try
        {
            ////Establish TCP/IP connection to server using URL entered////
            //Initializes a new instance of the TcpClient class and connects to the specified port (50040) on the specified host (localhost)
            commandSocket = new TcpClient("localhost", 50040);
            ////Set up communication streams////
            //Returns the NetworkStream (that has to be passed to StreamReader/Writer) used to send and receive data.
            commandStream = commandSocket.GetStream();
            //Implements a TextReader that reads characters from a byte stream in ASCII.
            commandReader = new StreamReader(commandStream, Encoding.ASCII);
            //Implements a TextWriter for writing characters to a stream in ASCII.
            ////Get initial response from server////
            //Reads a line of characters from the current stream and returns the data as a string.
            commandWriter = new StreamWriter(commandStream, Encoding.ASCII);
            response = commandReader.ReadLine();
            commandReader.ReadLine();   //get extra line terminator
            connected = true;   //indicate that we are connected
            // RepositoryOfThings.delsys_connected = 1;
        }
        catch
        {
            connected = false;
            // RepositoryOfThings.emg_enabled = false;
            Debug.Log("Trigno IM is not connected");
        }
    }
    //Send a command to the server and get the response
    public string SendCommand(string command)
    {
        string response = "";
        //Check if connected
        if (connected)
        {
            //Send the command
            //commandLine.Text = command;
            commandWriter.WriteLine(command);
            commandWriter.WriteLine();  //terminate command
            commandWriter.Flush();  //make sure command is sent immediately (Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream)
            //Read the response line and display
            response = commandReader.ReadLine();
            commandReader.ReadLine();   //get extra line terminator
                                        //responseLine.Text = response;
            Debug.Log("Delsys is connected! Your message has been");
        }
        else
            Debug.Log("Not connected.");
        return response;    //return the response we got
    }
    public void CloseConnection()
    {
        commandReader.Close();
        commandSocket.Close();
    }
}