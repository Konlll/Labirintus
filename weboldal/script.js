document.querySelector("button").addEventListener("click", function() {
    document.querySelectorAll("p, h1, h2, h3").forEach(function(el) {
      el.innerHTML = "English";
    });
  });