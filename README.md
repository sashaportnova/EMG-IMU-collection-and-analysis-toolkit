# EMG-IMU Data Collection & Analysis Toolkit (Unity + Delsys)

This Unity-based toolkit helps researchers collect, visualize, and analyze EMG and IMU data using **Delsys** hardware. Built for flexibility, rapid prototyping, and academic research, this tool simplifies the process of working with high-channel-count EMG systems.

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

The toolkit is organized into **three Unity scenes**: **Data Collection**, **Data Visualization**, and **Data Analysis**.

### 1. Data Collection Scene

- Connect to your Delsys Trigno system and verify the connection.
- Pre-load channel names via `ChannelLabels.txt` (e.g., associating each channel with a specific muscle).
- Name each trial before recording.
- Start and stop data collection per trial.
- Visualize **up to 16 raw EMG channels** in real time.

### 2. Data Visualization Scene

- Select from previously collected EMG trials.
- Load pre-defined channel names.
- Adjust the amplitude scaling of raw EMG plots (scaling may need to be as high as `1,000,000`).
- Start / Stop / Pause playback for each channel independently.

### 3. Data Analysis Scene

- Load any collected EMG trial.
- Optionally load an MVC file for EMG normalization.
- Choose from the following analysis options:
  - 📈 Plot filtered, normalized EMG
  - 📊 Plot histogram of filtered signals
  - 📉 View average and maximum EMG values per channel

---

## 💡 Notes

- **Channel Layout**:  
  Channels are split visually by side — **odd-numbered** channels on the **left**, **even-numbered** on the **right** — to reflect bilateral sensor placement (e.g., left/right legs). Future versions may include alternate layout options.

- **Amplitude Scaling**:  
  EMG signals often require scaling up to `1,000,000` to be visible in Unity plots. This will be improved in future iterations for more intuitive control.

- **Signal Centering**:  
  Raw EMG data is automatically **de-meaned** during both collection and visualization to center the signal around 0.

- **Custom Channel Labels**:  
  Edit the `ChannelLabels.txt` file in the `Assets/` folder to assign names to channels. Format:
1 TibialisAnterior_L
2 TibialisAnterior_R
...


If no name is provided, the channel will be displayed as unnamed.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

I hope this toolkit proves useful for fellow researchers using Delsys tools and helps streamline your academic and experimental workflows.
  
