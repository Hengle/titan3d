using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EditorCommon.Controls.Curves
{
    /// <summary>
    /// ��ͷ����
    /// </summary>
    public abstract class CurveBase : Shape
    {
        #region DependencyProperty

        /// <summary>
        /// ��ͷ���߼нǵ���������
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register(
            "ArrowAngle",
            typeof(double),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ��ͷ���ȵ���������
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register(
            "ArrowLength",
            typeof(double),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ��ͷ���ڶ˵���������
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register(
            "ArrowEnds",
            typeof(ArrowEnds),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(ArrowEnds.None, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ��ͷ���ڶ˵���������
        /// </summary>
        public static readonly DependencyProperty HalfArrowsProperty = DependencyProperty.Register(
            "HalfArrows",
            typeof(ArrowHalf),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(ArrowHalf.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ��ͷ�Ƿ�պϵ���������
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register(
            "IsArrowClosed",
            typeof(bool),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
            "StartPoint",
            typeof(Point),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// ������
        /// </summary>
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint",
            typeof(Point),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion DependencyProperty
        #region Fields
        /// <summary>
        /// ������״(������ͷ�;�����״)
        /// </summary>
        private readonly PathGeometry geometryWhole = new PathGeometry();

        /// <summary>
        /// ��ȥ��ͷ��ľ�����״
        /// </summary>
        protected readonly PathFigure figureConcrete = new PathFigure();

        /// <summary>
        /// ��ʼ���ļ�ͷ�߶�
        /// </summary>
        protected readonly PathFigure figureStart = new PathFigure();

        /// <summary>
        /// �������ļ�ͷ�߶�
        /// </summary>
        protected readonly PathFigure figureEnd = new PathFigure();

        #endregion Fields

        #region Constructor

        /// <summary>
        /// ���캯��
        /// </summary>
        protected CurveBase()
        {
            var polyLineSegStart = new PolyLineSegment();
            this.figureStart.Segments.Add(polyLineSegStart);

            var polyLineSegEnd = new PolyLineSegment();
            this.figureEnd.Segments.Add(polyLineSegEnd);

            this.geometryWhole.Figures.Add(this.figureConcrete);
            this.geometryWhole.Figures.Add(this.figureStart);
            this.geometryWhole.Figures.Add(this.figureEnd);
        }
        #endregion Constructor

        #region Properties

        /// <summary>
        /// ��ͷ���߼н�
        /// </summary>
        public double ArrowAngle
        {
            get { return (double)this.GetValue(ArrowAngleProperty); }
            set { this.SetValue(ArrowAngleProperty, value); }
        }

        /// <summary>
        /// ��ͷ���ߵĳ���
        /// </summary>
        public double ArrowLength
        {
            get { return (double)this.GetValue(ArrowLengthProperty); }
            set { this.SetValue(ArrowLengthProperty, value); }
        }

        /// <summary>
        /// ��ͷ���ڶ�
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            get { return (ArrowEnds)this.GetValue(ArrowEndsProperty); }
            set { this.SetValue(ArrowEndsProperty, value); }
        }
        /// <summary>
        /// ��ͷ���ڶ�
        /// </summary>
        public ArrowHalf HalfArrows
        {
            get { return (ArrowHalf)this.GetValue(HalfArrowsProperty); }
            set { this.SetValue(HalfArrowsProperty, value); }
        }
        /// <summary>
        /// ��ͷ�Ƿ�պ�
        /// </summary>
        public bool IsArrowClosed
        {
            get { return (bool)this.GetValue(IsArrowClosedProperty); }
            set { this.SetValue(IsArrowClosedProperty, value); }
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public Point StartPoint
        {
            get { return (Point)this.GetValue(StartPointProperty); }
            set { this.SetValue(StartPointProperty, value); }
        }
        /// <summary>
        /// ������
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)this.GetValue(EndPointProperty); }
            set { this.SetValue(EndPointProperty, value); }
        }

        /// <summary>
        /// ������״
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                this.figureConcrete.StartPoint = this.StartPoint;

                // ��վ�����״,�����ظ����
                this.figureConcrete.Segments.Clear();
                var segements = this.FillFigure();
                if (segements != null)
                {
                    foreach (var segement in segements)
                    {
                        this.figureConcrete.Segments.Add(segement);
                    }
                }

                CalculateArrow();
                return this.geometryWhole;
            }
        }

        #endregion Properties

        #region Protected Methods

        /// <summary>
        /// ��ȡ������״�ĸ�����ɲ���
        /// </summary>
        /// <returns>PathSegment����</returns>
        protected abstract PathSegmentCollection FillFigure();

        /// <summary>
        /// ��ȡ��ʼ��ͷ���Ľ�����
        /// </summary>
        /// <returns>��ʼ��ͷ���Ľ�����</returns>
        protected abstract Point GetStartArrowEndPoint();

        /// <summary>
        /// ��ȡ������ͷ���Ŀ�ʼ��
        /// </summary>
        /// <returns>������ͷ���Ŀ�ʼ��</returns>
        protected abstract Point GetEndArrowStartPoint();

        /// <summary>
        /// ��ȡ������ͷ���Ľ�����
        /// </summary>
        /// <returns>������ͷ���Ľ�����</returns>
        protected abstract Point GetEndArrowEndPoint();

        #endregion  Protected Methods

        #region Private Methods

        /// <summary>
        /// ����������֮��������ͷ
        /// </summary>
        /// <param name="pathfig">��ͷ���ڵ���״</param>
        /// <param name="startPoint">��ʼ��</param>
        /// <param name="endPoint">������</param>
        protected void CalculateArrow(PathFigure pathfig, Point startPoint, Point endPoint)
        {
            var polyseg = pathfig.Segments[0] as PolyLineSegment;
            if (polyseg != null)
            {
                var matx = new Matrix();
                Vector vect = startPoint - endPoint;

                // ��ȡ��λ����
                vect.Normalize();
                vect *= this.ArrowLength;

                // ��ת�нǵ�һ��
                matx.Rotate(this.ArrowAngle / 2);

                // �����ϰ�μ�ͷ�ĵ�
                pathfig.StartPoint = endPoint + (vect * matx);

                polyseg.Points.Clear();
                if(HalfArrows == ArrowHalf.Up || HalfArrows == ArrowHalf.Both)
                    polyseg.Points.Add(endPoint);

                matx.Rotate(-this.ArrowAngle);

                // �����°�μ�ͷ�ĵ�
                if (HalfArrows == ArrowHalf.Down || HalfArrows == ArrowHalf.Both)
                    polyseg.Points.Add(endPoint + (vect * matx));
            }

            pathfig.IsClosed = this.IsArrowClosed;
        }

        #endregion Private Methods
        public virtual void CalculateArrow()
        {
            // ���ƿ�ʼ���ļ�ͷ
            if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
            {
                CalculateArrow(this.figureStart, this.GetStartArrowEndPoint(), this.StartPoint);
            }

            // ���ƽ������ļ�ͷ
            if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
            {
                CalculateArrow(this.figureEnd, this.GetEndArrowStartPoint(), this.GetEndArrowEndPoint());
            }
        }
    }
}
