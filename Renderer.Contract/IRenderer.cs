namespace Renderer.Contract
{
	public interface IRenderer
	{
		Margin Margin { get; set; }
		byte[] Render(float xMin, float xMax, float yMin, float yMax, int width, int height);
	}
}
