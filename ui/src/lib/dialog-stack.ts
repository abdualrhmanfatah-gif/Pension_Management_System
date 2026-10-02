// Shared by dialog dismissal, focus trapping, and form keyboard shortcuts.
export interface DialogLayer {
  parent: DialogLayer | null;
  element: HTMLElement | null;
}

const layers: DialogLayer[] = [];
const handledEvents = new WeakSet<Event>();

export function registerDialogLayer(layer: DialogLayer, element: HTMLElement) {
  layer.element = element;
  // React mounts child effects before parent effects. Keep ancestors below children.
  const descendantIndex = layers.findIndex(candidate => {
    for (let parent = candidate.parent; parent; parent = parent.parent) {
      if (parent === layer) return true;
    }
    return false;
  });
  layers.splice(descendantIndex < 0 ? layers.length : descendantIndex, 0, layer);
  return () => {
    const index = layers.indexOf(layer);
    if (index >= 0) layers.splice(index, 1);
    layer.element = null;
  };
}

export function isTopmostDialog(element: HTMLElement | null) {
  return !!element && layers.at(-1)?.element === element;
}

export function isAnyDialogOpen() {
  return layers.length > 0;
}

export function claimDialogEvent(event: Event) {
  if (handledEvents.has(event)) return false;
  handledEvents.add(event);
  return true;
}
