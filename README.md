# EMG-IMU Data Collection & Analysis Toolkit (Unity + Delsys)

This Unity-based toolkit helps researchers collect, visualize, and analyze EMG and IMU data using **Delsys** hardware. Built for flexibility, rapid prototyping, and academic research, this tool simplifies the process of working with high-channel EMG/IMU systems. This toolkit was developed with support and in collaboration with all the wonderful people at the [Neuromechanics & Mobility Lab](https://steelelab.me.uw.edu/) at the [University of Washington](https://www.washington.edu/), where we focus on designing and evaluating technologies to support individuals with disabilities under the guidance of [Dr. Kat Steele](https://www.me.washington.edu/facultyfinder/kat-m-steele). Our research spans biomechanics, assistive technology, and human-centered design to improve mobility and function across the lifespan.

## Features

- 🎛️ **Live Data Streaming**
  - Real-time visualization of raw EMG signals (up to 16 channels).
  
- 📁 **Trial Management**
  - Save multiple trials with custom names and timestamps.

- 💾 **Data Storage**
  - Export synchronized EMG and IMU data to `.txt` files for offline analysis.

- 📊 **Data Loading & Review**
  - Load previously recorded files to visualize raw signals or perform basic analyses.

- 🧮 **EMG Analysis Tools**
  - Bandpass filtering and normalization
  - Histogram plotting
  - Channel-wise mean and max value computation

---

## 🔧 How It Works

### Delsys Connection

For this Unity interface to work, you will need the Delsys 16-channel base and the Trigno Control Utility software. Before running the interface, ensure your base is connected and Control Utility is up and running.

The toolkit is organized into **three Unity scenes**: **Data Collection**, **Data Visualization**, and **Data Analysis**.
<img width="914" height="513" alt="image" src="https://github.com/user-attachments/assets/01d972d9-cac0-4539-8c51-5ee0cb51eab8" />

### 1. Data Collection Scene

- Connect to your Delsys Trigno system and verify the connection by clicking on "Connect to Delsys".
- Pre-load channel names via `ChannelLabels.txt` (e.g., associating each channel with a specific muscle) by clicking on "Load Channel Labels".
- Name each trial before recording (use the Trial Label input field).
- Start and stop data collection for a trial (collects both EMG and IMU data at their respective sampling frequencies).
- Visualize **up to 16 raw EMG channels** in real time.
- Scale the EMG using either the EMG Scale input field (the values can be extremely high to be able to see the changes in potential) or once you found a suitable scale, you can adjust by using the EMG Multiplier on the right-hand side simply by dragging the slider.

<img width="902" height="507" alt="image" src="https://github.com/user-attachments/assets/b1031d7d-8dc5-4aef-bed9-cbfd628b8cff" />

### 2. Data Visualization Scene

- Select from previously collected EMG trials (pre-populated Trial Name dropdown list).
- Load pre-defined channel names ("Load EMG File").
- Pre-load channel names via `ChannelLabels.txt` (e.g., associating each channel with a specific muscle) by clicking on "Load Channel Labels".
- Adjust the amplitude scaling of raw EMG plots (scaling may need to be as high as `1,000,000`) using EMG Scale input field.
- I have also implemented a multiplier for the EMG signals without the need to enter extreme numbers into the "scale" input field. Just drag the slider from 1 to 10 and see your scaling change.
- Start / Stop / Pause playback for up to 16 channels. Empty channels will be displayed as 0.
- Don't forget to press "Load EMG File" again if a new file is selected before pressing "START".

<img width="910" height="497" alt="image" src="https://github.com/user-attachments/assets/8cd09c46-533b-4699-acd3-cfff63db1433" />

### 3. Data Analysis Scene

- Load an MVC file for EMG normalization by pressing "Load MVC File." The file must be named MVC.txt and be in the Data folder to be recognized by the script. If it says NOT AVAILABLE, make sure the correct file is in the correct folder.
- Load any collected EMG trial from the list of available trials (pre-populated dropdown menu) by clicking "Load Trial Data".
- Choose from the following analysis options:
  - 📈 Plot filtered, normalized EMG
  - 📊 Plot histogram of filtered signals
  - 📉 View average or maximum EMG values per channel
- Analyze the trial by clicking "Analyze Data".
 
<img width="896" height="501" alt="image" src="https://github.com/user-attachments/assets/33fc4e3d-ec9f-44e9-9e30-96bd6d8dfdb9" />


---

## 💡 Notes

- **Channel Layout**:  
  Channels are split visually by side — **odd-numbered** channels on the **left**, **even-numbered** on the **right** — to reflect bilateral sensor placement (e.g., left/right legs). Future versions may include alternate layout options.

- **Amplitude Scaling**:  
  EMG signals often require scaling up to `1,000,000` to be visible in Unity plots. I've implemented the slider for EMG Multiplier for a more intuitive control. So first find a good scaling value and then play around with the slider. The default scale is set to `1,000,000`.

- **Signal Centering**:  
  Raw EMG data is automatically **de-meaned** during both collection and visualization to center the signal around 0.

- **Custom Channel Labels**:  
  Edit the `ChannelLabels.txt` file in the `Assets/` folder to assign names to channels. Format:
1 TibialisAnterior_L
2 TibialisAnterior_R
...

If no name is provided, the channel will be displayed as unnamed. But the txt file MUST include the 1-16 numbers.

- **Data Collection**:  
  This script collects both EMG and IMU data as separate files. EMG files are saved in the "Data/" folder inside the "Assets/" with the same name as the corresponding trial, the IMU files will have the "_IMU" added to   the trial name.

- **Future Updates**:  
  This project is released as a free tool for any researchers out there who might be in need of a simplified interface for Delsys sensor data collection. If people are interested in adding their own functionality, feel free to add onto it or reach out and ask if it can be simply implemented. I'd be happy to support this in my free time (if I have it).
  

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

I hope this toolkit proves useful for fellow researchers using Delsys tools and helps streamline your academic and experimental workflows.
  
