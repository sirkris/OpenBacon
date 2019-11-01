using Reddit.Controllers;
using Xamarin.Forms;

namespace OpenBacon
{
    public class CommentData
    {
        public Comment Comment { get; set; }
        public int Index { get; set; }
        public Frame Frame { get; set; }
        public Grid Grid { get; set; }
        public TapGestureRecognizer TapComment { get; set; }
        public TapGestureRecognizer TapCollapse { get; set; }
        public SwipeGestureRecognizer SwipeLeft { get; set; }
        public SwipeGestureRecognizer SwipeRight { get; set; }
        public Label StatsLabel { get; set; }
        public int Score { get; set; }
        public bool IsCollapsed { get; set; }

        public CommentData(Comment comment, int index, Frame frame, Grid grid, TapGestureRecognizer tapComment, TapGestureRecognizer tapCollapse,
            SwipeGestureRecognizer swipeLeft, SwipeGestureRecognizer swipeRight, Label statsLabel, int? score = null, bool isCollapsed = false)
        {
            Comment = comment;
            Index = index;
            Frame = frame;
            Grid = grid;
            TapComment = tapComment;
            TapCollapse = tapCollapse;
            SwipeLeft = swipeLeft;
            SwipeRight = swipeRight;
            StatsLabel = statsLabel;
            Score = score ?? comment.Score;
            IsCollapsed = isCollapsed;
        }

        public CommentData() { }
    }
}
