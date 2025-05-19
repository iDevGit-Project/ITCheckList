function calculateTextDimensions(text, font = '15px IRANYekanXFaNum-Regular', maxWidth = window.innerWidth * 0.9) {
    const div = document.createElement('div');
    div.style.position = 'absolute';
    div.style.visibility = 'hidden';
    div.style.font = font;
    div.style.fontSize = '15px';
    div.style.whiteSpace = 'normal';
    div.style.lineHeight = '1.7';
    div.style.direction = 'rtl';
    div.style.textAlign = 'right';
    div.style.maxWidth = maxWidth + 'px';
    div.style.padding = '20px'; // حاشیه مشابه swal
    div.innerHTML = text.replace(/\n/g, '<br>');

    document.body.appendChild(div);
    const dimensions = {
        width: div.offsetWidth,
        height: div.offsetHeight
    };
    document.body.removeChild(div);

    return dimensions;
}
