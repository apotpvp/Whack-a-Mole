// Concepts:
//  AnimatedPictureBox is a derived PictureBox which support (whether or not scrolling) animation of frames, 
//   where one frame can consist of 1 or more images in a certain order.
//  You can control number of images horizontally and vertically and in which order (horizontal first then vertically or vice versa) to draw.
//  Images can be loaded from a spritesheet, set of filenames or folder.
//  You can control the speed of the animation. Also the number of subframes to draw between frames when scrolling.
//  You can control the direction of scrolling (up, down, left and right) and animation (forward or backwards).
//  There are multiple animation playing modes (once, looped, bounce).
//
//  Frame:          1 or more images in a certain order and layout
//  SubFrame:       internal custom drawn frame which consists of parts of current and next frame
//  Image:          image
//  SpriteSheet:    collection of images layout in a grid

// To do:
//  V support for non indexed pictures (see slot_machine_fruit (indexed).png)
//  - refactor subframes en frames -> zet gewoon alle frames of subframes in een playable frames array
//  - scrolling links/rechts en up/down kunnen gebruik maken van dezelfde subframes
//     left/right -> vertical scrolling icm forward/backwards playing direction
//     up/down    -> horizontal scrolling icm forward/backwards playing direction
//  - ease functions bij start/stop toevoegen (Standard en custom ability) https://gist.github.com/adrianseeley/4242677
//  - frames/spritesheet kunnen hergebruiken bij meerdere instanties van APB (bijv. 3 fruitrollen slotmachine)
//  - rotation
//  - bij meerdere images per frame, kan het zijn dat de laatste frame niet genoeg images heeft. 
//     Bijv. 8 images en 3 images per frame, houd je er 2 over in de laatste frame oftwel 1 lege plek
//     bij vertical scrolling is alleen de breedte tussen 2 frames belangrijk, als je laatste frame compacter maakt
//     en breedte veranderd niet dan zou dat een mogelijkheid moeten zijn
//     oplossing: (default) keepEmptySpotsInFrame, fillEmptySpotsInFrame, removeEmptySpotsInFrame
//     let op bij removeEmptySpotsInFrame: je kunt niet altijd alle empty spots removen. Bij VScrolling moet de breedte
//     na removen zelfde blijven, of bij HScrolling de hoogte.

// Bugs:
//  v FPS speed icm Scrolling (bij init) gaat nog iets niet goed
//  v Stop
//  v als bij einde anim bent (hoge frameindex) en je verhoogt aantal images in frame dan is de index out of bounds


using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

namespace AnimatedPictureBoxLibrary
{
    public enum PlayMode
    {
        Once,
        Looped,
        Bounce
    }

    public enum PlayingDirection
    {
        Forwards,
        Backwards
    }

    public enum SpriteSheetImageOrdering
    {
        LeftToRight,
        TopToBottom
    }

    public enum ScrollingDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum ImagesInFrameOrdering
    {
        HorizontalThenVertical,
        VerticalThenHorizontal
    }

    public class AnimatedPictureBox : PictureBox
    {
        public event EventHandler SubFrameChanged;
        public event EventHandler FrameChanged;
        public event EventHandler PlayingDirectionChanged;

        public int FrameWidth
        {
            get { return NumImagesInFrameHorizontally * ImageWidth; }
        }

        public int FrameHeight
        {
            get { return NumImagesInFrameVertically * ImageHeight; }
        }

        private int _currentSubFrameIndex=-1;
        public int CurrentSubFrameIndex
        {
            get { return _currentSubFrameIndex; }
            private set
            {
                if (_currentSubFrameIndex != value)
                {
                    _currentSubFrameIndex = value;
                    OnSubFrameChanged();
                }
            }
        }

        private Color backgroundColorFrame;
        public Color BackgroundColorFrame
        {
            get { return backgroundColorFrame; }
            set {
                if (value != backgroundColorFrame)
                {
                    backgroundColorFrame = value;
                    GenerateFrames();
                }
            }
        }

        private int numImagesInFrameVertically=1;
        public int NumImagesInFrameVertically
        {
            get { return numImagesInFrameVertically; }
            set {
                if (numImagesInFrameVertically!=value && value>0) {
                    numImagesInFrameVertically = value;
                    GenerateFrames();
                }
            }
        }

        private int numImagesInFrameHorizontally=1;
        public int NumImagesInFrameHorizontally
        {
            get { return numImagesInFrameHorizontally; }
            set
            {
                if (numImagesInFrameHorizontally != value && value > 0)
                {
                    numImagesInFrameHorizontally = value;
                    GenerateFrames();
                }
            }
        }

        private int numSubFramesToDrawWhenScrolling=1;
        public int NumSubFramesToDrawWhenScrolling
        {
            get { return numSubFramesToDrawWhenScrolling; }
            set {
                if (numSubFramesToDrawWhenScrolling!=value && value > 0)
                {
                    numSubFramesToDrawWhenScrolling = value;
                    double d = 1.0 / FPS / value * 1000;
                    interval = (int)Math.Round(d);
                    SetTimerInterval();
                    GenerateSubFrames();
                }
            }
        }

        private ScrollingDirection scrollingDirection;
        public ScrollingDirection ScrollingDirection
        {
            get { return scrollingDirection; }
            set {
                if (value != scrollingDirection)
                {
                    if (value == ScrollingDirection.None)
                        _currentSubFrameIndex = -1;
                    scrollingDirection = value;
                    GenerateSubFrames();
                }
            }
        }

        public bool IsVerticallyScrolling
        {
            get
            {
                return ScrollingDirection == ScrollingDirection.Up || ScrollingDirection == ScrollingDirection.Down;
            }
        }
        public bool IsHorizontallyScrolling
        {
            get
            {
                return ScrollingDirection == ScrollingDirection.Left || ScrollingDirection == ScrollingDirection.Right;
            }
        }

        private ImagesInFrameOrdering imagesInFrameOrdering;

        public ImagesInFrameOrdering ImagesInFrameOrdering
        {
            get { return imagesInFrameOrdering; }
            set
            {
                if (value != imagesInFrameOrdering)
                {
                    imagesInFrameOrdering = value;
                    GenerateFrames();
                }
            }
        }

        private int imageWidth;
        public int ImageWidth
        {
            get { return imageWidth; }
        }

        private int imageHeight;
        public int ImageHeight
        {
            get { return imageHeight; }
        }

        private bool IsScrolling
        {
            get { return ScrollingDirection != ScrollingDirection.None; }
        }

        public bool IsPlaying
        {
            get {
                if (PlayMode == PlayMode.Once && PlayingDirection == PlayingDirection.Forwards && CurrentFrameIndex == NumImages - 1) return false;
                if (PlayMode == PlayMode.Once && PlayingDirection == PlayingDirection.Backwards && CurrentFrameIndex == 0) return false;
                return tmrNextDrawEvent.Enabled && NumFrames > 0;
            }
        }

        private int _currentFrameIndex = -1;
        public int CurrentFrameIndex
        {
            get { return _currentFrameIndex; }
            private set {
                if (_currentFrameIndex != value) {
                    _currentFrameIndex = value;
                    OnFrameChanged();
                }
            }
        }

        private int TotalSubFrames {
            get {
                return NumFrames * NumSubFramesToDrawWhenScrolling;
            }
        }

        public int NumFrames
        {
            get
            {
                return (int)Math.Ceiling((double)NumImages / (double)(NumImagesInFrameHorizontally * NumImagesInFrameVertically));
            }
        }

        private int numImages=0;
        public int NumImages
        {
            get { return numImages; }
        }

        private PlayMode playMode = PlayMode.Looped;
        public PlayMode PlayMode
        {
            get { return playMode; }
            set { playMode = value; }
        }

        private double fps;
        public double FPS
        {
            get { return fps; }
            set {
                if (value > 0)
                {
                    fps = value;
                    interval = (int)Math.Round(1000 / value);
                    SetTimerInterval();
                }
            }
        }

        private void SetTimerInterval()
        {
            if (IsScrolling)
            {
                //set to an subframe interval
                tmrNextDrawEvent.Interval = (int)Math.Round(1000 / FPS / NumSubFramesToDrawWhenScrolling);
            }
            else
                tmrNextDrawEvent.Interval = interval;
        }

        private int interval;
        public int Interval
        {
            get { return interval; }
            set {
                if (value > 0)
                {
                    interval = value;
                    fps = 1000 / interval;
                    SetTimerInterval();
                }
            }
        }

        private PlayingDirection _playingDirection = PlayingDirection.Forwards;
        public PlayingDirection PlayingDirection
        {
            get { return _playingDirection; }
            set
            {
                if (_playingDirection != value)
                {
                    _playingDirection = value;
                    OnPlayingDirectionChanged();
                }
            }
        }

        private Timer tmrNextDrawEvent = new Timer();
        private Bitmap[] images;
        private Bitmap[] frames;
        private Bitmap[] subframes;

        public AnimatedPictureBox()
        {
            tmrNextDrawEvent.Tick += tmrNextFrame_Tick;
        }

        public AnimatedPictureBox(List<string> filenamesFrames): this()
        {
            SetImages(filenamesFrames);
        }

        public void Start()
        {
            if (NumFrames > 0)
            {
                //(re)start at 0 or resume
                if (CurrentFrameIndex == -1) CurrentFrameIndex = 0;
                if (IsScrolling && CurrentSubFrameIndex == -1) CurrentSubFrameIndex = 0;

                Start(CurrentFrameIndex);
            }
        }

        public void Start(int frameNr)
        {
            DebugLogger.Log("Start with frameNr " + frameNr);

            if (frameNr != CurrentFrameIndex && IsScrolling) CurrentSubFrameIndex = 0;
            GotoFrame(frameNr);
            tmrNextDrawEvent.Enabled = true;
        }

        public void Start(int frameNr, int subFrameNr)
        {
            DebugLogger.Log(String.Format("Start with frameNr {0} and subFrameNr ",frameNr, subFrameNr));

            GotoFrame(frameNr, subFrameNr);
            tmrNextDrawEvent.Enabled = true;
        }

        public void Stop()
        {
            DebugLogger.Log("Stop");

            tmrNextDrawEvent.Enabled = false;
            GotoFrame(-1);
        }

        public void Pause()
        {
            DebugLogger.Log("Pause");

            tmrNextDrawEvent.Enabled = false;
        }

        private void GenerateFrames()
        {
            if (NumFrames == 0) return;

            //reset
            CurrentFrameIndex = 0;
            CurrentSubFrameIndex = 0;

            frames = new Bitmap[NumFrames];
            int numImagesInFrame = NumImagesInFrameHorizontally * NumImagesInFrameVertically;

            DebugLogger.Log(String.Format("GenerateFrames with {0} NumImages / {1} numImagesInFrame = {2} NumFrames", NumImages, numImagesInFrame, NumFrames));

            for (int i = 0; i < NumFrames; i++)
            {
                frames[i] = new Bitmap(FrameWidth, FrameHeight, images[0].PixelFormat);

                Bitmap destFrame = frames[i];
                BitmapTools.FillBitmapWithColor(destFrame, backgroundColorFrame);

                Rectangle srcRect = new Rectangle(0, 0, ImageWidth, ImageHeight); //always same dimensions
                if (ImagesInFrameOrdering == ImagesInFrameOrdering.HorizontalThenVertical)
                {
                    for (int y = 0; y < NumImagesInFrameVertically; y++)
                    {
                        for (int x = 0; x < NumImagesInFrameHorizontally; x++)
                        {
                            int imageIndex = i * numImagesInFrame + y * NumImagesInFrameHorizontally + x;
                            if (imageIndex < NumImages)
                            {
                                Bitmap srcImage = images[imageIndex];
                                Rectangle destRect = new Rectangle(x * ImageWidth, y * ImageHeight, ImageWidth, ImageHeight);
                                //copy srcRect from image to destRect in frame
                                BitmapTools.CopyImage(srcImage, srcRect, destFrame, destRect);
                            }
                        }
                    }
                }
                if (ImagesInFrameOrdering == ImagesInFrameOrdering.VerticalThenHorizontal)
                {
                    for (int x = 0; x < NumImagesInFrameHorizontally; x++)
                    {
                        for (int y = 0; y < NumImagesInFrameVertically; y++)
                        {
                            int imageIndex = i * numImagesInFrame + x * NumImagesInFrameVertically + y;
                            if (imageIndex < NumImages)
                            {
                                Bitmap srcImage = images[imageIndex];
                                Rectangle destRect = new Rectangle(x * ImageWidth, y * ImageHeight, ImageWidth, ImageHeight);
                                //copy srcRect from image to destRect in frame
                                BitmapTools.CopyImage(srcImage, srcRect, destFrame, destRect);
                            }
                        }
                    }
                }
            }

            GenerateSubFrames();
        }

        private void GenerateSubFrames()
        {
            if (IsScrolling && TotalSubFrames > 0)
            {
                DebugLogger.Log(String.Format("GenerateSubFrames with {0} TotalSubFrames", TotalSubFrames));

                //reset
                CurrentSubFrameIndex = 0;

                subframes = new Bitmap[TotalSubFrames];
                //generate subframes for frame 0..NumFrames-1..0
                for (int fi = 0; fi < NumFrames; fi++)
                {
                    int frameIndex1 = fi;
                    int frameIndex2 = (fi+1) % NumFrames; //wrap around

                    Bitmap frame1 = frames[frameIndex1];
                    Bitmap frame2 = frames[frameIndex2];

                    subframes[fi* NumSubFramesToDrawWhenScrolling] = frames[frameIndex1]; //first subframe is always the same as frame

                    //generate second subframe (and further) between frame1 and frame2
                    for (int sfi = 1; sfi < NumSubFramesToDrawWhenScrolling; sfi++)
                    {
                        Bitmap subframe = new Bitmap(FrameWidth,FrameHeight, subframes[0].PixelFormat);

                        Size size1;
                        Size size2;
                        Point srcPoint1, destPoint1;
                        Point srcPoint2, destPoint2;

                        if (IsVerticallyScrolling)
                        {
                            int numLinesFrame1 = (int)Math.Round(FrameHeight * (1 - (double) sfi / (double)NumSubFramesToDrawWhenScrolling));
                            int numLinesFrame2 = FrameHeight - numLinesFrame1;
                            size1 = new Size(FrameWidth, numLinesFrame1);
                            size2 = new Size(FrameWidth, numLinesFrame2);

                            if (ScrollingDirection == ScrollingDirection.Down)
                            {
                                srcPoint1 = new Point(0, 0);
                                destPoint1 = new Point(0, numLinesFrame2);
                                srcPoint2 = new Point(0, numLinesFrame1);
                                destPoint2 = new Point(0, 0);
                            }
                            else //ScrollingDirection.Up
                            {
                                srcPoint1 = new Point(0, numLinesFrame2);
                                destPoint1 = new Point(0, 0);
                                srcPoint2 = new Point(0, 0);
                                destPoint2 = new Point(0, numLinesFrame1);
                            }
                        }

                        else //IsHorizontallyScrolling
                        {
                            int numLinesFrame1 = (int)Math.Round(FrameWidth * (1 - (double)sfi / (double)NumSubFramesToDrawWhenScrolling));
                            int numLinesFrame2 = FrameWidth - numLinesFrame1;
                            size1 = new Size(numLinesFrame1, FrameHeight);
                            size2 = new Size(numLinesFrame2, FrameHeight);

                            if (ScrollingDirection == ScrollingDirection.Left)
                            {
                                srcPoint1 = new Point(numLinesFrame2, 0); 
                                destPoint1 = new Point(0, 0);
                                srcPoint2 = new Point(0, 0);
                                destPoint2 = new Point(numLinesFrame1, 0); 
                            }
                            else //ScrollingDirection.Right
                            {
                                srcPoint1 = new Point(0, 0); 
                                destPoint1 = new Point(numLinesFrame2, 0);
                                srcPoint2 = new Point(numLinesFrame1, 0);
                                destPoint2 = new Point(0, 0); 
                            }
                        }

                        Rectangle srcRect1 = new Rectangle(srcPoint1, size1);
                        Rectangle destRect1 = new Rectangle(destPoint1, size1);
                        Rectangle srcRect2 = new Rectangle(srcPoint2, size2);
                        Rectangle destRect2 = new Rectangle(destPoint2, size2);

                        BitmapTools.CopyImage(frame1, srcRect1, subframe, destRect1);
                        BitmapTools.CopyImage(frame2, srcRect2, subframe, destRect2);

                        subframes[fi * NumSubFramesToDrawWhenScrolling + sfi] = subframe;
                    }
                }
            }
        }

        public void SetImages(List<string> filenamesFrames)
        {
            int numImages = filenamesFrames.Count;

            DebugLogger.Log("SetImages with " + numImages);

            if (numImages > 0)
            {
                Bitmap[] images = new Bitmap[numImages];

                int imageWidth = 0, imageHeight = 0;
                for (int i = 0; i < numImages; i++)
                {
                    Bitmap image = new Bitmap(filenamesFrames[i]);

                    //check if all images are same dimensions
                    if (i > 0)
                    {
                        if ((image.Width != imageWidth) || (image.Height != imageHeight))
                            throw new Exception("Not all images have same width and/or height");
                    }
                    else
                    {
                        imageWidth = image.Width;
                        imageHeight = image.Height;
                    }

                    images[i] = image;
                }

                this.imageWidth = imageWidth;
                this.imageHeight = imageHeight;
                this.images = images;
                this.numImages = numImages;
                CurrentFrameIndex = -1;

                GenerateFrames();
            }
        }

        public void SetImagesBySpritesheet(string filenameSpritesheet, int numImagesHorizontal, int numImagesVertical, SpriteSheetImageOrdering spriteSheetOrdering)
        {
            SetImagesBySpritesheet(filenameSpritesheet, numImagesHorizontal, numImagesVertical, spriteSheetOrdering, numImagesHorizontal * numImagesVertical);
        }

        public void SetImagesBySpritesheet(string filenameSpritesheet,int numImagesHorizontal, int numImagesVertical, SpriteSheetImageOrdering spriteSheetOrdering, int nrOfImagesToLoad)
        {
            bool IsBitSet(int b, int pos)
            {
                return (b & (1 << pos)) != 0;
            }

            int numImages = Math.Min(numImagesHorizontal * numImagesVertical,nrOfImagesToLoad);

            DebugLogger.Log("SetImagesBySpritesheet with " + numImages);

            if (numImages > 0)
            {
                Bitmap orig = new Bitmap(filenameSpritesheet);
                Bitmap spritesheet;

                //convert image if necessary
                if (IsBitSet((int)orig.PixelFormat, (int)PixelFormat.Indexed))
                {
                    spritesheet = new Bitmap(orig.Width, orig.Height, PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(spritesheet))
                    {
                        gr.DrawImage(orig, new Rectangle(0, 0, spritesheet.Width, spritesheet.Height));
                    }
                }
                else
                    spritesheet = orig;

                int imageWidth = spritesheet.Width / numImagesHorizontal;
                int imageHeight = spritesheet.Height / numImagesVertical;

                Bitmap[] images = new Bitmap[numImages];

                if (spriteSheetOrdering == SpriteSheetImageOrdering.LeftToRight)
                {
                    bool done = false;
                    for (int y = 0; y < numImagesVertical; y++)
                    {
                        for (int x = 0; x < numImagesHorizontal; x++)
                        {
                            Rectangle rectSrc = new Rectangle(x * imageWidth, y * imageHeight, imageWidth, imageHeight);
                            Bitmap image = BitmapTools.GetImageFromSpriteSheet(spritesheet, rectSrc);
                            int i = y * numImagesHorizontal + x;
                            images[i] = image;

                            if (i >= numImages - 1)
                            {
                                done = true;
                                break;
                            }
                        }
                        if (done) break;
                    }
                }

                if (spriteSheetOrdering == SpriteSheetImageOrdering.TopToBottom)
                {
                    for (int x = 0; x < numImagesHorizontal; x++)
                    {
                        for (int y = 0; y < numImagesVertical; y++)
                        {
                            Rectangle rectSrc = new Rectangle(x * imageWidth, y * imageHeight, imageWidth, imageHeight);
                            Bitmap image = BitmapTools.GetImageFromSpriteSheet(spritesheet, rectSrc);
                            int i = x * numImagesVertical + y;
                            images[i] = image;
                        }
                    }
                }

                this.imageWidth = imageWidth;
                this.imageHeight = imageHeight;
                this.images = images;
                this.numImages = numImages;
                CurrentFrameIndex = -1;

                GenerateFrames();
            }
        }

        private void tmrNextFrame_Tick(object sender, EventArgs e)
        {
            DebugLogger.Log("tmrNextFrame_Tick");

            if (IsScrolling)
            {
                NextSubFrame();
            }
            else
            {
                NextFrame();
            }
        }

        public void NextSubFrame()
        {
            CurrentSubFrameIndex = CalculateNextSubFrameIndex(CurrentSubFrameIndex);

            DebugLogger.Log("NextSubFrame with CurrentSubFrameIndex "+ CurrentSubFrameIndex);

            if (CurrentSubFrameIndex == 0)
            {
                CurrentFrameIndex = CalculateNextFrameIndex(CurrentFrameIndex);
            }
            ShowCurrentSubFrame();
        }

        public void NextFrame()
        {
            CurrentFrameIndex = CalculateNextFrameIndex(CurrentFrameIndex);

            DebugLogger.Log("NextFrame with CurrentFrameIndex " + CurrentFrameIndex);

            ShowCurrentFrame();
        }

        private void ShowCurrentSubFrame()
        {
            if (CurrentSubFrameIndex >= 0)
            {
                int subframeIndex = CurrentFrameIndex * NumSubFramesToDrawWhenScrolling + CurrentSubFrameIndex;

                DebugLogger.Log(String.Format("ShowCurrentSubFrame with CurrentFrameIndex {0} CurrentSubFrameIndex {0} (subframe #{1})", CurrentFrameIndex, CurrentSubFrameIndex, subframeIndex));

                this.Image = subframes[subframeIndex];
            }
            else {
                DebugLogger.Log("ShowCurrentSubFrame with EMPTY image");
                this.Image = null;
            }
        }

        private void ShowCurrentFrame()
        {
            if (CurrentFrameIndex >= 0)
            {
                DebugLogger.Log(String.Format("ShowCurrentFrame with CurrentFrameIndex {0} (CurrentSubFrameIndex {1})", CurrentFrameIndex, CurrentSubFrameIndex));
                this.Image = frames[CurrentFrameIndex];
            }
            else
            {
                DebugLogger.Log("ShowCurrentFrame with EMPTY image");
                this.Image = null;
            }
        }

        private int CalculateNextSubFrameIndex(int currentSubFrameIndex)
        {
            return (currentSubFrameIndex + 1) % NumSubFramesToDrawWhenScrolling;
        }

        private int CalculateNextFrameIndex(int currentFrameIndex)
        {
            switch (PlayMode)
            {
                case PlayMode.Once:
                    if ((PlayingDirection == PlayingDirection.Forwards) &&
                        (currentFrameIndex < NumFrames - 1)) currentFrameIndex++;
                    if ((PlayingDirection == PlayingDirection.Backwards) &&
                        (currentFrameIndex > 0)) currentFrameIndex--;
                    break;

                case PlayMode.Looped:
                    if (PlayingDirection == PlayingDirection.Forwards)
                    {
                        if (currentFrameIndex == NumFrames - 1)
                            currentFrameIndex = 0;
                        else
                            currentFrameIndex++;
                    }
                    if (PlayingDirection == PlayingDirection.Backwards)
                    {
                        if (currentFrameIndex == 0)
                            currentFrameIndex = NumFrames - 1;
                        else
                            currentFrameIndex--;
                    }
                    break;

                case PlayMode.Bounce:
                    if (PlayingDirection == PlayingDirection.Forwards)
                    {
                        if (currentFrameIndex == NumFrames - 1)
                        {
                            currentFrameIndex--;
                            PlayingDirection = PlayingDirection.Backwards;
                        }
                        else currentFrameIndex++;
                    }
                    else
                        if (PlayingDirection == PlayingDirection.Backwards)
                        {
                            if (currentFrameIndex == 0)
                            {
                                currentFrameIndex++;
                                PlayingDirection = PlayingDirection.Forwards;
                            }
                            else currentFrameIndex--;
                        }
                    break;
            }
            return currentFrameIndex;
        }

        public void GotoFrame(int frameIndex)
        {
            GotoFrame(frameIndex, CurrentSubFrameIndex);
        }

        public void GotoFrame(int frameIndex, int subFrameIndex)
        {
            DebugLogger.Log(String.Format("GotoFrame with frameIndex {0} and subFrameIndex {1}", frameIndex, subFrameIndex));

            if (frameIndex >= 0 && frameIndex < NumFrames)
            {
                CurrentFrameIndex = frameIndex;

                if (IsScrolling)
                {
                    if (subFrameIndex>=0 && subFrameIndex < NumSubFramesToDrawWhenScrolling)
                    {
                        CurrentSubFrameIndex = subFrameIndex;
                        ShowCurrentSubFrame();
                    }
                    else throw new Exception(String.Format("Frame {0} with subframe {1} doesn't exist", frameIndex, subFrameIndex));
                }
                else
                    ShowCurrentFrame();
            }
            //else
            //    throw new Exception(String.Format("Frame {0} doesn't exist", frameIndex));
        }

        public void OnFrameChanged()
        {
            DebugLogger.Log("OnFrameChanged");
            EventHandler handler = FrameChanged;
            if (null != handler) handler(this, EventArgs.Empty);
        }

        public void OnSubFrameChanged()
        {
            DebugLogger.Log("OnSubFrameChanged");
            EventHandler handler = SubFrameChanged;
            if (null != handler) handler(this, EventArgs.Empty);
        }

        public void OnPlayingDirectionChanged()
        {
            DebugLogger.Log("OnPlayingDirectionChanged");
            EventHandler handler = PlayingDirectionChanged;
            if (null != handler) handler(this, EventArgs.Empty);
        }

        public Bitmap GetImage(int imageNr)
        {
            if (imageNr >= 0 && imageNr <= NumImages) return images[imageNr];
            else return null;
        }

        public Bitmap GetFrame(int frameNr)
        {
            if (frameNr >= 0 && frameNr <= NumFrames) return frames[frameNr];
            else return null;
        }

        public Bitmap GetSubFrame(int subframeNr)
        {
            if (subframeNr >= 0 && subframeNr <= NumSubFramesToDrawWhenScrolling) return subframes[subframeNr];
            else return null;
        }
    }
}
