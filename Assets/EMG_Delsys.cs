using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
//using System.Runtime;
using System;
using System.Timers;
public class EMG_Delsys : MonoBehaviour
{
    //Class parameters
    private string fname;
    //TCP/IP parameters
    private BinaryReader emgReader;
    private TcpClient emgSocket;
    private NetworkStream emgStream;
    //EMG and IMU parameters
    private int NumberofTrignos;
    private int NumberofRows;
    private float[] raw_emg;
    //File parameters
    private string path;
    private string[] temp_raw_file;
    public static ConcurrentQueue<string> EMG_queue = new ConcurrentQueue<string>();
    //Protocol to run
    //Parameter to avoid race conditions
    private UnityEngine.Object thisLock = new UnityEngine.Object();
    public static float[] lastSample = new float[16];
    public static string lastSampleEMG;
    public void Main()
    {
        NumberofTrignos = 16;
        raw_emg = new float[NumberofTrignos];
        temp_raw_file = new string[NumberofTrignos];
        //here I have to use the TCP/IP port that Trigno uses for EMG channels
        emgSocket = new TcpClient("localhost", 50043);
        emgSocket.NoDelay = true;
        emgStream = emgSocket.GetStream();
        //############################
        //############################
        //ACQUISITION: MAIN BODY
        //############################
        //############################
        emgReader = new BinaryReader(emgStream);
        while (DataCollection.connectToDelsys)
        {
            // for EMG
            int EMGavailable = emgSocket.Available;  // should be 896 Bytes after transition
                                                     //swenvEMG.WriteLine(Convert.ToString(available));
                                                     //Console.WriteLine(Convert.ToString(available));
                                                     //UnityEngine.Debug.Log("availability: " + EMGavailable.ToString());
            if (EMGavailable > 0)
            {
                int Ncycles = EMGavailable / 64; //64 bytes each cycle (16 channels * 4 bytes per channel)
                for (int n = 0; n < Ncycles; n++)
                {
                    //
                    //Demultiplex the data
                    for (int sn = 0; sn < NumberofTrignos; ++sn)
                    {
                        raw_emg[sn] = emgReader.ReadSingle();
                        temp_raw_file[sn] = raw_emg[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                                                                                                                     //EMG_queue_calibration.Enqueue(raw_emg)
                    }
                    if (DataCollection.dataCollectionFlag)
                    {
                        lock (thisLock)
                        {
                            lastSample = raw_emg;
                            lastSampleEMG = string.Join("\t", temp_raw_file);
                            //UnityEngine.Debug.Log(lastSampleEMG);
                        }
                        string s_raw = string.Join("\t", temp_raw_file);
                        EMG_queue.Enqueue(s_raw);
                    }
                }
            }
        }
    }
    void OnApplicationQuit()
    {
        emgReader.Close();
        emgSocket.Close();
        UnityEngine.Debug.Log("Your EMG data collection has finished.");
    }
}