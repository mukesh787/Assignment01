using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;
[RequireComponent(typeof(ARRaycastManager))]
public class TapToPlaceCube : MonoBehaviour
{

 public GameObject gameobjectToInstantiate;
 private GameObject spawnedObject;
 // Reference to ARRaycastManager
 private ARRaycastManager _RaycastManager;
 private Vector2 touchPosition;

 // List to store raycast hits
 static List<ARRaycastHit> hits = new List<ARRaycastHit>();

 // Reference to ARCameraManager
 private ARCameraManager _CameraManager;

 // Reference to a UI RawImage element to display the captured image
 public UnityEngine.UI.RawImage rawImage;

 // A flag to indicate if an image capture is requested
 private bool captureRequested = false;

 // A counter to wait for 10 frames after placing the cube
 private int frameCounter = 0;

 // Start is called before the first frame update
 private void Awake()
 {
 // Get ARRaycastManager component
 _RaycastManager = GetComponent<ARRaycastManager>();
 // Get ARCameraManager component
 _CameraManager = GetComponent<ARCameraManager>();
 }

 bool TryGetTouchPosition(out Vector2 touchPosition){
 if(Input.touchCount>0){
 touchPosition = Input.GetTouch(0).position;
 return true;
 }
 touchPosition=default;
 return false;
 }

 // Update is called once per frame
 void Update()
 {
 if(!TryGetTouchPosition(out Vector2 touchPosition)){
 return;
 }
 // Check if user has touched the screen

 // Perform a raycast from touch position against detected planes
 if (_RaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
 {
 // Get the first hit result
 var hitPose = hits[0].pose;

 if(spawnedObject==null){
 spawnedObject=Instantiate(gameobjectToInstantiate, hitPose.position, hitPose.rotation);
 // Set the capture flag to true
 captureRequested = true;
 }
 else{
 spawnedObject.transform.position=hitPose.position;
 }
 }
 
 // If capture is requested, increment the frame counter
 if(captureRequested){
 frameCounter++;
 }

 // If frame counter reaches 10, capture the image and reset the flag and counter
 if(frameCounter == 10){
 CaptureImage();
 captureRequested = false;
 frameCounter = 0;
 }
 }

 // Method to capture the image from AR camera
 void CaptureImage(){
 // Request an image from AR camera manager
 _CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image);

 // Convert the image to RGBA format
 var conversionParams = new XRCpuImage.ConversionParams{
 inputRect = new RectInt(0, 0, image.width, image.height),
 outputDimensions = new Vector2Int(image.width, image.height),
 outputFormat = TextureFormat.RGBA32,
 transformation = XRCpuImage.Transformation.None
 };

 // Create a texture to hold the converted image
 var texture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);

 // Get the raw texture data and apply it to the texture
 var rawTextureData = texture.GetRawTextureData<byte>();
 image.Convert(conversionParams, rawTextureData);
 texture.Apply();

 

 // Display the texture on the UI element
 //rawImage.texture = texture;

 // Optionally, save the texture to a file
 byte[] bytes = texture.EncodeToPNG();
 File.WriteAllBytes(Application.persistentDataPath + "/ARCapture.png", bytes);

 // Dispose the image after conversion
 image.Dispose();
 }
}
