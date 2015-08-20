using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace fzp
{
    public partial class MainForm : RibbonForm
    {
        public MainForm()
        {
            InitializeComponent();
            saveFileDialog1.Filter = @"Bitmap Images (*.bmp)|*.bmp|All Files (*.*)|*.*";
            BoxWidth = 18; // in
            BoxHeight = 3; // in
            BoxDeep = 12; // in
            Smooth = false;
            DpiX = DpiY = 600; // dpi
            WaveLength = 500; // nm
        }

        /// <summary>
        ///     Computed OpenCV image
        /// </summary>
        private Image<Gray, float> Image { get; set; }

        /// <summary>
        ///     Unlike a standard lens, a binary zone plate produces intensity maxima along the axis of the plate at odd fractions
        ///     (f/3, f/5, f/7, etc.). Although these contain less energy (counts of the spot) than the principal focus (because it
        ///     is wider), they have the same maximum intensity (counts/m^2).
        ///     However, if the zone plate is constructed so that the opacity varies in a gradual, sinusoidal manner, the resulting
        ///     diffraction causes only a single focal point to be formed. This type of zone plate pattern is the equivalent of a
        ///     transmission hologram of a converging lens.
        /// </summary>
        private bool Smooth
        {
            get { return barToggleSwitchSmooth.Checked; }
            set { barToggleSwitchSmooth.Checked = value; }
        }

        private float WaveLength
        {
            get { return (float) barEditWaveLength.EditValue; }
            set { barEditWaveLength.EditValue = value; }
        }

        private float BoxWidth
        {
            get { return (float) barEditX.EditValue; }
            set { barEditX.EditValue = value; }
        }

        private float BoxHeight
        {
            get { return (float) barEditY.EditValue; }
            set { barEditY.EditValue = value; }
        }

        private float BoxDeep
        {
            get { return (float) barEditZ.EditValue; }
            set { barEditZ.EditValue = value; }
        }

        /// <summary>
        ///     Dots-Per-Inch
        ///     X - axe
        /// </summary>
        private float DpiX
        {
            get { return (float) barEditDpiX.EditValue; }
            set { barEditDpiX.EditValue = value; }
        }

        /// <summary>
        ///     Dots-Per-Inch
        ///     Y - axe
        /// </summary>
        private float DpiY
        {
            get { return (float) barEditDpiY.EditValue; }
            set { barEditDpiY.EditValue = value; }
        }

        /// <summary>
        ///     Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (pictureEdit1.Image == null) return;
                saveFileDialog1.FileName = BoxWidth + "x" + BoxHeight + "x" + BoxDeep + "x" + WaveLength;
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                pictureEdit1.Image.Save(saveFileDialog1.FileName);
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Build_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var width = (int) Math.Ceiling(DpiX*BoxWidth);
                var height = (int) Math.Ceiling(DpiY*BoxHeight);

                Image = new Image<Gray, float>(width, height);

                using (var builder = new FzpBuilder
                {
                    PlateMode =
                        Smooth ? FzpBuilder.FresnelZonePlateMode.Sinusoidal : FzpBuilder.FresnelZonePlateMode.Binary,
                    DpiX = DpiX,
                    DpiY = DpiY,
                    Distance = BoxDeep,
                    WaveLength = WaveLength
                })
                {
                    builder.Fill(Image.Data);
                    Bitmap bitmap = Image.Bitmap;
                    bitmap.SetResolution(DpiX, DpiY);
                    pictureEdit1.Image = bitmap;
                }
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void iAbout_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var aboutBox = new AboutBox())
                aboutBox.ShowDialog();
        }

        /// <summary>
        ///     Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Show_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                using (var imageViewer = new ImageViewer(Image, "FZP"))
                    imageViewer.ShowDialog();
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }
    }
}