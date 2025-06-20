namespace Renderer.Contract
{
	public interface IRenderer
	{
		byte[] Render(float xMin, float xMax, float yMin, float yMax, int width, int height);
	}
}
