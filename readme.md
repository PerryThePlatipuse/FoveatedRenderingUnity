# Foveated Rendering for Non-VR Games - Unity Plugin

## Overview

This project brings **Foveated Rendering (FR)** to traditional PC games running on standard monitors - extending beyond the typical use in VR/AR. FR boosts performance by rendering full detail only at the center of vision, aligning with the human eye's sharpest focus (fovea), and reducing detail in the periphery.

By combining **webcam-based gaze tracking** with **real-time rendering optimizations**, this Unity plugin enables dynamic, gaze-driven detail adjustment, resulting in smoother gameplay and higher framerates without sacrificing visual quality.

---

## 👀 Gaze Tracking

- **Beam SDK**  
  The plugin supports Beam Eye Tracker via SDK which is a proprietary solution, but has great performance

- **EyeGestures**  
  We also have support of and open source python library EyeGestures for gaze tracking.

## Rendering Optimisations
- **Variable Rate Shading (VRS)**  
  Plugin support's Nvidia's VRS technology for FR.

- **LOD-Based Rendering**  
  It is a custom optimization that enables FR via LOD optimisation.

---

## Requirements
- DirectX 11 support
- Nvidia GPU with VRS support 
- 2k monitor

## Setup Instructions

### Beam Installation   
- Download Beam from official [website](https://beam.eyeware.tech/)
- To use Beam with the plugin, complete the calibration procedure and enable "Game Extensions" setting in Beam.

### EyeGestures Installation
- Install python package
  ```bash
  python3 -m pip install eyeGestures
  ```
- run *EyeGesturesImpl.py* script and complete the calibration procedure

### Demo Installation
- Download **build** folder from the [project's folder](https://disk.yandex.ru/client/disk/%D0%9A%D1%83%D1%80%D1%81%D0%BE%D0%B2%D0%BE%D0%B9%20%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%20%D0%A5%D0%BE%D1%80%D1%82%20%D0%A9%D0%B5%D1%80%D0%B1%D0%B0%D0%BA%D0%BE%D0%B2)
- Run file *Demov5.exe*

## Plugin Usage

To use it, you should attach VrsUrpController (VrsBirpController) script to a camera object.

## Results

Preliminary benchmarks show noticeable performance gains:

| Scene          | FPS Improvement |
|----------------|------------------|
| Night City     | +18.0%           |
| Mountains      | +32.5%           |

---

## Future Improvements

- **Luminance-Contrast-Aware FR**  
  Smarter region blending using brightness and contrast heuristics.

- **Predictive Gaze Models**  
  Preemptively render in expected gaze zones using motion forecasting.

- **DLSS / Upscaling Integration**  
  Combine with AI upscalers for both speed and image fidelity.
