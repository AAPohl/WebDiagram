export class ViewportController {
    constructor({ imgElement, getMargin, updateViewPort, updateSize }) {
        this.img = imgElement;
        this.getMargin = getMargin;
        this.updateViewPort = updateViewPort;
        this.updateSize = updateSize;

        this.viewPort = { xMin: 0, yMin: 0, xMax: 1, yMax: 1 };
        this.stepX = 0.02;
        this.stepY = 0.02;
        this.zoomFactor = 1.1;
        this.isDragging = false;
        this.lastMousePos = { x: 0, y: 0 };
    }

    setInitialViewPort(viewPort) {
        this.viewPort = viewPort;
        this.updateViewPort(viewPort);
    }

    startDrag(e) {
        this.isDragging = true;
        this.lastMousePos = { x: e.clientX, y: e.clientY };
        e.preventDefault();
    }

    stopDrag() {
        this.isDragging = false;
    }

    dragMove(e) {
        if (!this.isDragging) return;
        const margin = this.getMargin();

        const dx = e.clientX - this.lastMousePos.x;
        const dy = e.clientY - this.lastMousePos.y;
        this.lastMousePos = { x: e.clientX, y: e.clientY };

        const widthPx = this.img.naturalWidth - margin.left - margin.right;
        const heightPx = this.img.naturalHeight - margin.top - margin.bottom;

        const xRange = this.viewPort.xMax - this.viewPort.xMin;
        const yRange = this.viewPort.yMax - this.viewPort.yMin;

        const scaleX = xRange / widthPx;
        const scaleY = yRange / heightPx;

        this.viewPort.xMin -= dx * scaleX;
        this.viewPort.xMax -= dx * scaleX;
        this.viewPort.yMin += dy * scaleY;
        this.viewPort.yMax += dy * scaleY;

        this.updateViewPort(this.viewPort);
    }

    handleKeyDown(e) {
        if (!this.viewPort) return;

        switch (e.key) {
            case 'ArrowLeft':
                this.viewPort.xMin += this.stepX;
                this.viewPort.xMax += this.stepX;
                break;
            case 'ArrowRight':
                this.viewPort.xMin -= this.stepX;
                this.viewPort.xMax -= this.stepX;
                break;
            case 'ArrowUp':
                this.viewPort.yMin -= this.stepY;
                this.viewPort.yMax -= this.stepY;
                break;
            case 'ArrowDown':
                this.viewPort.yMin += this.stepY;
                this.viewPort.yMax += this.stepY;
                break;
            default:
                return;
        }

        this.updateViewPort(this.viewPort);
    }

    handleWheel(e) {
        const margin = this.getMargin();
        const rect = this.img.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        const width = this.viewPort.xMax - this.viewPort.xMin;
        const height = this.viewPort.yMax - this.viewPort.yMin;
        const scale = e.deltaY < 0 ? 1 / this.zoomFactor : this.zoomFactor;

        const applyZoom = (worldX, worldY, newWidth, newHeight) => {
            const newXMin = worldX - ((mouseX - margin.left) / (rect.width - margin.left - margin.right)) * newWidth;
            const newYMax = worldY + ((mouseY - margin.top) / (rect.height - margin.top - margin.bottom)) * newHeight;
            this.viewPort = {
                xMin: newXMin,
                xMax: newXMin + newWidth,
                yMin: newYMax - newHeight,
                yMax: newYMax
            };
            this.updateViewPort(this.viewPort);
            e.preventDefault();
        };

        if (mouseY >= rect.height - margin.bottom) {
            const worldX = this.viewPort.xMin + ((mouseX - margin.left) / (rect.width - margin.left - margin.right)) * width;
            const newWidth = width * scale;
            applyZoom(worldX, 0, newWidth, height);
            return;
        }

        if (mouseX <= margin.left) {
            const worldY = this.viewPort.yMax - ((mouseY - margin.top) / (rect.height - margin.top - margin.bottom)) * height;
            const newHeight = height * scale;
            applyZoom(0, worldY, width, newHeight);
            return;
        }

        const insideH = mouseX > margin.left && mouseX < (rect.width - margin.right);
        const insideV = mouseY > margin.top && mouseY < (rect.height - margin.bottom);

        if (insideH && insideV) {
            const worldX = this.viewPort.xMin + ((mouseX - margin.left) / (rect.width - margin.left - margin.right)) * width;
            const worldY = this.viewPort.yMax - ((mouseY - margin.top) / (rect.height - margin.top - margin.bottom)) * height;
            const newWidth = width * scale;
            const newHeight = height * scale;
            applyZoom(worldX, worldY, newWidth, newHeight);
        }
    }
}
