window.patchbook = {
  async copy(text) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch {
      return false;
    }
  },
  showDialog(element) { element.showModal(); },
  closeDialog(element) { element.close(); },
  confirmLeave() { return window.confirm("Discard your unsaved changes?"); }
};
