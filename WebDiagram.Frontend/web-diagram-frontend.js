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
    }

    setViewPort(viewPort) {
        this.viewPort = viewPort;
        this.updateImage(viewPort);        
    }
    updateImage(viewPort) {
        const url = `${this.source}/render?xMin=${viewPort.xMin}&xMax=${viewPort.xMax}&yMin=${viewPort.yMin}&yMax=${viewPort.yMax}`;
        this.img.src = url;
    }
  }
  
  customElements.define('web-diagram-frontend', WebDiagramFrontend);