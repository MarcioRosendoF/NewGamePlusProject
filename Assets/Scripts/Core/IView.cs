namespace Core
{
    public interface IView
    {
        void Show();
        void Hide();
        void Toggle();
        bool IsVisible { get; }
    }
}
