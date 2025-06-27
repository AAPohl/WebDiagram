export class HoverController {
    constructor({ imgElement, getMargin, getViewPort, updateHover }) {
        this.img = imgElement;
        this.getMargin = getMargin;
        this.getViewPort = getViewPort;
        this.updateHover = updateHover;

        this.handleMouseMove = this.handleMouseMove.bind(this);
        this.img.addEventListener('mousemove', this.handleMouseMove);
    }

    destroy() {
        this.img.removeEventListener('mousemove', this.handleMouseMove);
    }

    handleMouseMove(e) {
        const margin = this.getMargin();
        const rect = this.img.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;

        const insideH = mouseX > margin.left && mouseX < (rect.width - margin.right);
        const insideV = mouseY > margin.top && mouseY < (rect.height - margin.bottom);

        if (!insideH || !insideV) {
            this.updateHover(null);
            return;
        }

        const viewPort = this.getViewPort();
        const xRange = viewPort.xMax - viewPort.xMin;
        const yRange = viewPort.yMax - viewPort.yMin;

        const normX = (mouseX - margin.left) / (rect.width - margin.left - margin.right);
        const normY = (mouseY - margin.top) / (rect.height - margin.top - margin.bottom);

        const worldX = viewPort.xMin + normX * xRange;
        const worldY = viewPort.yMax - normY * yRange;

        this.updateHover({ x: worldX, y: worldY });
    }
}
