window.patchbook = {
  downloadJson(filename, bytes) {
    const url = URL.createObjectURL(new Blob([bytes], { type: "application/json" }));
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    document.body.append(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  },
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
