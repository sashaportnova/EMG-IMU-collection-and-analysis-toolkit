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
public class IMU_Delsys : MonoBehaviour
{
    //Class parameters
    private string fname;
    //TCP/IP parameters
    private BinaryReader imuReader;
    private TcpClient imuSocket;
    private NetworkStream imuStream;
    //EMG and IMU parameters
    private int NumberofTrignos;
    private int NumberofRows;
    private float[] ax;
    private float[] ay;
    private float[] az;
    private float[] gx;
    private float[] gy;
    private float[] gz;
    private float[] mx;
    private float[] my;
    private float[] mz;
    //File parameters
    private string path;
    // private StreamWriter swIMU;
    private string[] temp_imu_file;
    public static ConcurrentQueue<string> IMU_queue = new ConcurrentQueue<string>();
    //Parameter to avoid race conditions
    private UnityEngine.Object thisLock = new UnityEngine.Object();
    public static string lastSampleIMU;
    public void Main()
    {
        NumberofTrignos = 16;
        temp_imu_file = new string[NumberofTrignos * 9];
        ax = new float[NumberofTrignos];
        ay = new float[NumberofTrignos];
        az = new float[NumberofTrignos];
        gx = new float[NumberofTrignos];
        gy = new float[NumberofTrignos];
        gz = new float[NumberofTrignos];
        mx = new float[NumberofTrignos];
        my = new float[NumberofTrignos];
        mz = new float[NumberofTrignos];
        //here I have to use the TCP/IP port that Trigno uses for IMU channels
        imuSocket = new TcpClient("localhost", 50044);
        imuSocket.NoDelay = true;
        imuStream = imuSocket.GetStream();
        //############################
        //############################
        //ACQUISITION: MAIN BODY
        //############################
        //############################
        imuReader = new BinaryReader(imuStream);
        while (DataCollection.connectToDelsys)
        {
            int IMUavailable = imuSocket.Available;//should be 896 Bytes after transition
            // UnityEngine.Debug.Log("availability:"+ IMUavailable.ToString());
            if (IMUavailable >= 0)
            {
                //2 cycles each 13 ms approximately (EMG has 14 cycles)
                int Ncycles = IMUavailable / 576; //576 bytes each cycle (144 channels * 4 bytes per channel)
                for (int n = 0; n < Ncycles; n++)
                {
                    int tmp_file = 0;
                    //Demultiplex the data and save for UI display (TCP/IP always transmit data as I'd have 16 Trigno sensors)
                    for (int sn = 0; sn < NumberofTrignos; ++sn)
                    {
                        ax[sn] = imuReader.ReadSingle();
                        ay[sn] = imuReader.ReadSingle();
                        az[sn] = imuReader.ReadSingle();
                        gx[sn] = (imuReader.ReadSingle() * Convert.ToSingle(Math.PI) / 180f);
                        gy[sn] = (imuReader.ReadSingle() * Convert.ToSingle(Math.PI) / 180f);
                        gz[sn] = (imuReader.ReadSingle() * Convert.ToSingle(Math.PI) / 180f);
                        mx[sn] = imuReader.ReadSingle();
                        my[sn] = imuReader.ReadSingle();
                        mz[sn] = imuReader.ReadSingle();
                        temp_imu_file[tmp_file] = ax[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 1] = ay[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 2] = az[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 3] = gx[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 4] = gy[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 5] = gz[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 6] = mx[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 7] = my[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        temp_imu_file[tmp_file + 8] = mz[sn].ToString(System.Globalization.CultureInfo.InvariantCulture); //string for file
                        tmp_file = tmp_file + 9; // every 9 reads is a new IMU sensor
                    }
                    if (DataCollection.dataCollectionFlag)
                    {
                        ///
                        lock (thisLock)
                        {
                            lastSampleIMU = string.Join("\t", temp_imu_file);
                        }
                        ///
                        // string s_imu = DateTime.UtcNow.ToString("yymmddHHmmssfff") + "\t" + string.Join("\t", temp_imu_file);
                        string s_imu = string.Join("\t", temp_imu_file);
                        IMU_queue.Enqueue(s_imu);
                    }
                }
            }
        }
    }
    void OnApplicationQuit()
    {
        imuReader.Close();
        imuSocket.Close();
        UnityEngine.Debug.Log("Your IMU data collection has finished.");
    }
}