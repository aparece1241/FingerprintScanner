using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Enrollment
{
    /* NOTE: This form is a base for the Enrollment Form
		All changes in the CaptureForm will be reflected in all its derived forms.
	*/
    //delegate void Function();

    public partial class CaptureFormVerify : Form, DPFP.Capture.EventHandler
	{
		public CaptureFormVerify()
		{
			InitializeComponent();
		}


		protected virtual void Init()
        {
            try
            {
				Capturer = new DPFP.Capture.Capture();              // Create a capture operation.

                if ( null != Capturer )
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    SetPrompt("Can't initiate capture operation!");
            }
            catch
            {               
                MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            
            }
		}

		protected virtual void Process(DPFP.Sample Sample)
		{
			// Draw fingerprint sample image.
			DrawPicture(ConvertSampleToBitmap(Sample));
		}

		protected void Start()
		{
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                }
                catch
                {
                    SetPrompt("Can't initiate capture!");
                }
            }
		}

		protected void Stop()
		{
            if (null != Capturer)
            {
                try
                {
                    Capturer.StopCapture();
                }
                catch
                {
                    SetPrompt("Can't terminate capture!");
                }
            }
		}
		
	#region Form Event Handlers:
		private void CaptureForm_Load(object sender, EventArgs e)
		{
			Init();
			Start();	// Start capture operation.
		}

		private void CaptureForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Stop();
		}
		#endregion

		#region EventHandler Members:
		public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample){
			Process(Sample);
		}
		public void OnFingerGone(object Capture, string ReaderSerialNumber) {
		}
		public void OnFingerTouch(object Capture, string ReaderSerialNumber) {
		}
		public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
		public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
		public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }
	#endregion

		protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
		{
			DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();	// Create a sample convertor.
			Bitmap bitmap = null;												            // TODO: the size doesn't matter
			Convertor.ConvertToPicture(Sample, ref bitmap);									// TODO: return bitmap as a result
			return bitmap;
		}

		protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
		{
			DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();	// Create a feature extractor
			DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
			DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
				return features;
			else
				return null;
		}
		
		protected void SetPrompt(string prompt)
		{
			this.Invoke(new Function(delegate() {
				lblStatus.Text = prompt;

				if (prompt != "Please Try Again!" && !prompt.Contains("Too"))
				{
					lblStatus.ForeColor = System.Drawing.Color.Green;
					lblStatus.BackColor = System.Drawing.Color.White;
				}
				else
				{
					lblStatus.ForeColor = System.Drawing.Color.Red;
					lblStatus.BackColor = System.Drawing.Color.White;
				}
			}));
		}

		protected void SetName(string name)
		{
			this.Invoke(new Function(delegate () {
				lblName.Text = name;
				lblName.BackColor = System.Drawing.Color.White;
			}));
		}

		protected void initializeFields()
		{
			this.Invoke(new Function(delegate () {
				lblName.Text = DateTime.Now.ToString("dddd");
				lblStatus.Text = "";
				lblStatus.BackColor = System.Drawing.Color.Transparent;
				lblName.BackColor = System.Drawing.Color.Transparent;
			}));
		}

		private void DrawPicture(Bitmap bitmap)
		{
			this.Invoke(new Function(delegate() {
				Picture.Image = new Bitmap(bitmap, Picture.Size);	// fit the image into the picture box
			}));
		}

		private DPFP.Capture.Capture Capturer;

        private void timer1_Tick(object sender, EventArgs e)
        {
			lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
		}

        private void lblTime_Click(object sender, EventArgs e)
        {

        }
    }
}