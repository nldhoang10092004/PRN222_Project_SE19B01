// ── FLASHCARD VIEWER MODULE ──
let currentFlashcards = [];
let currentIndex = 0;
let isFlipped = false;

function openFlashcardModal(setId) {
    // Fetch flashcards từ API
    fetch(`/learn/Course/Flashcards/${setId}`)
        .then(response => response.json())
        .then(data => {
            if (!data || data.length === 0) {
                alert('Bộ flashcards này chưa có thẻ nào.');
                return;
            }

            currentFlashcards = data;
            currentIndex = 0;
            isFlipped = false;

            renderFlashcard();
            updateNavButtons();
            updateCounter();

            document.getElementById('flashcardModal').classList.add('active');
        })
        .catch(error => {
            console.error('Error loading flashcards:', error);
            alert('Không thể tải flashcards. Vui lòng thử lại.');
        });
}

function closeFlashcardModal() {
    document.getElementById('flashcardModal').classList.remove('active');
    currentFlashcards = [];
    currentIndex = 0;
    isFlipped = false;
}

function renderFlashcard() {
    const card = currentFlashcards[currentIndex];
    const flashcardElement = document.getElementById('flashcard');
    const frontText = document.getElementById('flashcardFrontText');
    const backText = document.getElementById('flashcardBackText');

    frontText.textContent = card.front;
    backText.textContent = card.back;

    // Reset flip state
    if (isFlipped) {
        flashcardElement.classList.add('flipped');
    } else {
        flashcardElement.classList.remove('flipped');
    }
}

function flipCard() {
    isFlipped = !isFlipped;
    const flashcardElement = document.getElementById('flashcard');
    flashcardElement.classList.toggle('flipped');
}

function prevCard() {
    if (currentIndex > 0) {
        currentIndex--;
        isFlipped = false;
        renderFlashcard();
        updateNavButtons();
        updateCounter();
    }
}

function nextCard() {
    if (currentIndex < currentFlashcards.length - 1) {
        currentIndex++;
        isFlipped = false;
        renderFlashcard();
        updateNavButtons();
        updateCounter();
    }
}

function updateNavButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    prevBtn.disabled = currentIndex === 0;
    nextBtn.disabled = currentIndex === currentFlashcards.length - 1;
}

function updateCounter() {
    const counter = document.getElementById('flashcardCounter');
    counter.textContent = `${currentIndex + 1} / ${currentFlashcards.length}`;
}

// Keyboard navigation
document.addEventListener('keydown', function(e) {
    const modal = document.getElementById('flashcardModal');
    if (!modal.classList.contains('active')) return;

    switch(e.key) {
        case 'ArrowLeft':
            prevCard();
            break;
        case 'ArrowRight':
            nextCard();
            break;
        case ' ':
        case 'Enter':
            e.preventDefault();
            flipCard();
            break;
        case 'Escape':
            closeFlashcardModal();
            break;
    }
});
