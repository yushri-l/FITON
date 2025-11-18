// Sample Clothes Data for Testing
// Run this in browser console after logging in to add sample clothes

const sampleClothes = [
  // Tops
  {
    name: "Classic White Shirt",
    description: "A timeless white button-down shirt perfect for any occasion",
    category: "Business",
    brand: "FITON",
    size: "M",
    color: "White",
    type: "shirt",
    image: ""
  },
  {
    name: "Casual Blue T-Shirt",
    description: "Comfortable cotton t-shirt for everyday wear",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Blue",
    type: "t-shirt",
    image: ""
  },
  {
    name: "Elegant Black Blouse",
    description: "Sophisticated blouse perfect for formal events",
    category: "Formal",
    brand: "FITON",
    size: "M",
    color: "Black",
    type: "blouse",
    image: ""
  },
  {
    name: "Cozy Gray Sweater",
    description: "Warm and comfortable sweater for cool weather",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Gray",
    type: "sweater",
    image: ""
  },
  {
    name: "Navy Blue Blazer",
    description: "Professional blazer for business meetings",
    category: "Business",
    brand: "FITON",
    size: "M",
    color: "Navy",
    type: "blazer",
    image: ""
  },

  // Bottoms
  {
    name: "Dark Blue Jeans",
    description: "Classic straight-leg jeans that go with everything",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Dark Blue",
    type: "jeans",
    image: ""
  },
  {
    name: "Black Formal Pants",
    description: "Tailored pants perfect for business attire",
    category: "Business",
    brand: "FITON",
    size: "M",
    color: "Black",
    type: "pants",
    image: ""
  },
  {
    name: "Khaki Chinos",
    description: "Versatile chino pants for smart casual looks",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Khaki",
    type: "pants",
    image: ""
  },
  {
    name: "Pleated Mini Skirt",
    description: "Trendy pleated skirt for a youthful look",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Navy",
    type: "skirt",
    image: ""
  },
  {
    name: "Black Leggings",
    description: "Comfortable stretch leggings for active wear",
    category: "Sport",
    brand: "FITON",
    size: "M",
    color: "Black",
    type: "leggings",
    image: ""
  },

  // Full Outfits
  {
    name: "Little Black Dress",
    description: "Classic black dress suitable for any formal occasion",
    category: "Formal",
    brand: "FITON",
    size: "M",
    color: "Black",
    type: "dress",
    image: ""
  },
  {
    name: "Floral Summer Dress",
    description: "Light and airy dress perfect for summer days",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Floral",
    type: "dress",
    image: ""
  },
  {
    name: "Elegant Evening Gown",
    description: "Stunning gown for special occasions and events",
    category: "Formal",
    brand: "FITON",
    size: "M",
    color: "Burgundy",
    type: "gown",
    image: ""
  },
  {
    name: "Casual Denim Jumpsuit",
    description: "Trendy jumpsuit for a modern casual look",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Blue",
    type: "jumpsuit",
    image: ""
  },
  {
    name: "Bohemian Maxi Frock",
    description: "Free-flowing frock with bohemian style",
    category: "Casual",
    brand: "FITON",
    size: "M",
    color: "Multicolor",
    type: "frock",
    image: ""
  }
];

// Function to add all sample clothes
async function addSampleClothes() {
  const baseURL = 'http://localhost:5000'; // Adjust if your backend runs on different port
  const token = localStorage.getItem('token'); // Assuming token is stored in localStorage
  
  if (!token) {
    console.error('No authentication token found. Please log in first.');
    return;
  }

  console.log('Adding sample clothes...');
  
  for (const clothes of sampleClothes) {
    try {
      const response = await fetch(`${baseURL}/api/clothes`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(clothes)
      });
      
      if (response.ok) {
        const result = await response.json();
        console.log(`✅ Added: ${clothes.name}`);
      } else {
        console.error(`❌ Failed to add ${clothes.name}:`, await response.text());
      }
    } catch (error) {
      console.error(`❌ Error adding ${clothes.name}:`, error);
    }
  }
  
  console.log('🎉 Sample clothes data has been added!');
  console.log('Now you can create wardrobe outfits by combining these items.');
}

// Instructions
console.log('Sample Clothes Data Script Loaded!');
console.log('To add sample clothes, run: addSampleClothes()');
console.log('Make sure you are logged in to the application first.');