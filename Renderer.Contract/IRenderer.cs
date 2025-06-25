namespace Renderer.Contract
{
	public interface IRenderer
	{
		Margin Margin { get; }

		void Start();
		void Stop();
        void UpdateSize(int width, int height);
        void UpdateViewport(float xMin, float xMax, float yMin, float yMax);
    }
}
