Highcharts.Chart.prototype.callbacks.push(function(chart) {
  var hasTouch = hasTouch = document.documentElement.ontouchstart !== undefined,
      mousepointer = chart.pointer,
      container = chart.container,
      mouseMove;

  mouseMove = function (e) {
    // let the system handle multitouch operations like two finger scroll
    // and pinching
    if (e && e.touches && e.touches.length > 1) {
      return;
    }

    // normalize
//    e = mousepointer.normalizeMouseEvent(e);
    if (!hasTouch) { // not for touch devices
      e.returnValue = false;
    }

    var chartX = e.chartX,
      chartY = e.chartY,
      isOutsidePlot = !chart.isInsidePlot(chartX - chart.plotLeft, chartY - chart.plotTop);

    // cancel on mouse outside
    if (isOutsidePlot) {

      /*if (!lastWasOutsidePlot) {
        // reset the pointer
        resetpointer();
      }*/

      // drop the selection if any and reset mouseIsDown and hasDragged
      //drop();
      if (chartX < chart.plotLeft) {
        chartX = chart.plotLeft;
      } else if (chartX > chart.plotLeft + chart.plotWidth) {
        chartX = chart.plotLeft + chart.plotWidth;
      }

      if (chartY < chart.plotTop) {
        chartY = chart.plotTop;
      } else if (chartY > chart.plotTop + chart.plotHeight) {
        chartY = chart.plotTop + chart.plotHeight;
      }
    }

    if (chart.mouseIsDown && e.type !== 'touchstart') { // make selection

      // determine if the mouse has moved more than 10px
      hasDragged = Math.sqrt(
        Math.pow(mousepointer.mouseDownX - chartX, 2) +
        Math.pow(mousepointer.mouseDownY - chartY, 2)
      );
      if (hasDragged > 10) {
        var clickedInside = chart.isInsidePlot(mousepointer.mouseDownX - chart.plotLeft, mousepointer.mouseDownY - chart.plotTop);

        // make a selection
        if (chart.hasCartesianSeries && (mousepointer.zoomX || mousepointer.zoomY) && clickedInside) {
          if (!mousepointer.selectionMarker) {
            mousepointer.selectionMarker = chart.renderer.rect(
              chart.plotLeft,
              chart.plotTop,
              zoomHor ? 1 : chart.plotWidth,
              zoomVert ? 1 : chart.plotHeight,
              0
            )
            .attr({
              fill: mousepointer.options.chart.selectionMarkerFill || 'rgba(69,114,167,0.25)',
              zIndex: 7
            })
            .add();
          }
        }

        // adjust the width of the selection marker
        if (mousepointer.selectionMarker && zoomHor) {
          var xSize = chartX - mousepointer.mouseDownX;
          mousepointer.selectionMarker.attr({
            width: mathAbs(xSize),
            x: (xSize > 0 ? 0 : xSize) + mousepointer.mouseDownX
          });
        }
        // adjust the height of the selection marker
        if (mousepointer.selectionMarker && zoomVert) {
          var ySize = chartY - mousepointer.mouseDownY;
          mousepointer.selectionMarker.attr({
            height: mathAbs(ySize),
            y: (ySize > 0 ? 0 : ySize) + mousepointer.mouseDownY
          });
        }

        // panning
        if (clickedInside && !mousepointer.selectionMarker && mousepointer.options.chart.panning) {
          chart.pan(chartX);
        }
      }

    } else if (!isOutsidePlot) {
      // show the tooltip
      mousepointer.onmousemove(e);
    }

    lastWasOutsidePlot = isOutsidePlot;
  }

  container.onmousemove = container.ontouchstart = container.ontouchmove = mouseMove;
});