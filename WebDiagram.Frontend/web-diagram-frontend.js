class WebDiagramFrontend extends HTMLElement {
    connectedCallback() {
        const img = document.createElement('img');
        img.id = "view";
        img.width = this.getAttribute("width") || 100;
        img.height = this.getAttribute("height") || 100;
        img.style.border = "2px solid #555";
        img.style.background = "black";

        this.appendChild(img);
        this.img = img;
        this.source = this.getAttribute("source")

        fetch(`${this.source}/config`)
            .then(res => res.json())
            .then(cfg => {
                this.margin = cfg.margin;
        });

        this.viewPort = { xMin: 0, yMin: 0, xMax: 1, yMax: 1 };
        this.isDragging = false;
        this.lastMousePos = { x: 0, y: 0 };

        this.stepX = 0.02;
        this.stepY = 0.02;

        img.addEventListener("mousedown", this.startDrag.bind(this));
        window.addEventListener("mouseup", this.stopDrag.bind(this));
        window.addEventListener("mousemove", this.dragMove.bind(this));
        window.addEventListener("keydown", this.handleKeyDown.bind(this));
    }

    setInitialViewPort(viewPort) {
        this.viewPort = viewPort;
        this.updateImage(viewPort);        
    }
    updateImage(viewPort) {
        const url = `${this.source}/render?xMin=${viewPort.xMin}&xMax=${viewPort.xMax}&yMin=${viewPort.yMin}&yMax=${viewPort.yMax}`;
        this.img.src = url;
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
    
        const dx = e.clientX - this.lastMousePos.x;
        const dy = e.clientY - this.lastMousePos.y;
        this.lastMousePos = { x: e.clientX, y: e.clientY };
    
        const widthPx = this.img.naturalWidth - this.margin.left - this.margin.right;
        const heightPx = this.img.naturalHeight - this.margin.top - this.margin.bottom;
    
        const xRange = this.viewPort.xMax - this.viewPort.xMin;
        const yRange = this.viewPort.yMax - this.viewPort.yMin;
    
        const scaleX = xRange / widthPx;
        const scaleY = yRange / heightPx;
    
        this.viewPort.xMin -= dx * scaleX;
        this.viewPort.xMax -= dx * scaleX;
    
        this.viewPort.yMin += dy * scaleY;
        this.viewPort.yMax += dy * scaleY;
    
        this.updateImage(this.viewPort);
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
    
        this.updateImage(this.viewPort);
    }
    
}
  
customElements.define('web-diagram-frontend', WebDiagramFrontend);