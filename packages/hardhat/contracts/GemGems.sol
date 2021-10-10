// SPDX-License-Identifier: MIT
pragma solidity ^0.8.2;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@chainlink/contracts/src/v0.8/VRFConsumerBase.sol";

contract GemGemsDiamond is VRFConsumerBase,ERC721, Pausable, Ownable {
    using Counters for Counters.Counter;
    
    bytes32 internal keyHash;
    Counters.Counter private _tokenIdCounter;
    
    mapping(address => uint256) public ownerBalances;
    mapping(address => bool) private isFundRecipient;
    
    address[] private owners;
    address charityAddress;
    
    uint256 internal fee;
    
    uint256 public randomResult;
    mapping(uint256=>DnaGenerator.DiamondDna) public DiamondDna;

    constructor(address[] memory _owners, address _charityAddress) ERC721("GemGemsDiamond", "GGD")  VRFConsumerBase(
            0xdD3782915140c8f3b190B5D67eAc6dc5760C46E9, // VRF Coordinator
            0xa36085F69e2889c224210F603D836748e7dC0088  // LINK Token
        )
    {
        keyHash = 0x6c3699283bda56ad74f6b855546325b68d482e983852a7a82979cc4807b641f4;
        fee = 0.1 * 10 ** 18; // 0.1 LINK (Varies by network)
        
        owners = _owners;
        charityAddress = _charityAddress;
    }
    
    /**
     * Constructor inherits VRFConsumerBase
     * 
     * Network: Kovan
     * Chainlink VRF Coordinator address: 0xdD3782915140c8f3b190B5D67eAc6dc5760C46E9
     * LINK token address:                0xa36085F69e2889c224210F603D836748e7dC0088
     * Key Hash: 0x6c3699283bda56ad74f6b855546325b68d482e983852a7a82979cc4807b641f4
     */
   
    
    /** 
     * Requests randomness 
     */
    function getRandomNumber() public onlyOwner returns (bytes32 requestId) {
        require(randomResult==0,"Already done");
        require(LINK.balanceOf(address(this)) >= fee, "No LINK");
        return requestRandomness(keyHash, fee);
    }
    
    /**
     * Callback function used by VRF Coordinator
     */
    function fulfillRandomness(bytes32 requestId, uint256 randomness) internal override {
        randomResult = randomness;
    }

    fallback() external payable { 
       revert("no eth split here");
    }
    
    receive() external payable { 
       revert("no eth split here");
    }
    
    // function withdrawBalance() external {} - Implement a withdraw function to avoid locking your LINK in the contract
     // function withdrawLink() external {} - Implement a withdraw function to avoid locking your LINK in the contract
    
    
    function _baseURI() internal pure override returns (string memory) {
        return "https://gemgems.io/diamonds/";
    }

    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }

    function safeMint() public payable whenNotPaused {
      require(randomResult !=0, "no random yet");
      require(_tokenIdCounter.current() < 10000, "limit reached");
      
      require(msg.value >= 70000000000000000,"0.07 mint fee");
      
      dnaMint(msg.sender,1);
    }
     
     function dnaMint(address to, uint256 polishCount) private {
        uint256 tokenId = _tokenIdCounter.current();
        _safeMint(to, tokenId);
        
        (DnaGenerator.DiamondDna memory dna, uint256 newRnd) = DnaGenerator.getDna(randomResult, polishCount);
        
        DiamondDna[tokenId] = dna;
        randomResult = newRnd;
        
        _tokenIdCounter.increment();
        
        FeeSplitter.distributeFunds(msg.value, owners ,charityAddress, ownerBalances);
    }
    
    function burnAndMint(uint256 tokenToBurn) public payable whenNotPaused {
        require(ERC721.ownerOf(tokenToBurn) == msg.sender,"Burn only yours");
        require(DiamondDna[tokenToBurn].PolishCount < 6,"5 polishes allowed");
        
        uint256 polishFee = 70000000000000000 * DiamondDna[tokenToBurn].PolishCount + 1;
        
        require(msg.value >= polishFee, string(abi.encodePacked(polishFee, " mint fee")));
        
        _burn(tokenToBurn);
        delete DiamondDna[tokenToBurn];
        
        dnaMint(msg.sender, DiamondDna[tokenToBurn].PolishCount + 1);
    }

    function _beforeTokenTransfer(address from, address to, uint256 tokenId)
        internal
        whenNotPaused
        override(ERC721)
    {
        super._beforeTokenTransfer(from, to, tokenId);
    }

    // The following functions are overrides required by Solidity.

    function _burn(uint256 tokenId) internal override(ERC721) {
        super._burn(tokenId);
    }
    
    function tokenURI(uint256 tokenId)
        public
        view
        override(ERC721)
        returns (string memory)
    {
        return super.tokenURI(tokenId);
    }

    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC721)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }
}

library DnaGenerator{
    /**
     * @dev Converts a `uint256` to its ASCII `string` decimal representation.
     */
     
    struct DiamondDna{
         uint256 Colour;
         uint256 CutShape;
         uint256 Clarity;
         uint256 CutQuality;
         uint256 Carat;
         uint256 RandomValue;
         uint256 PolishCount;
    }
     
    function getDna(uint256 randomValue,uint256 polishCount) internal pure returns (DiamondDna memory returnDna, uint256 nextRnd) {
        
        DiamondDna memory dna;
        
        dna.RandomValue = randomValue;
        
        uint256 milColour = randomValue % 1000000;
        if(milColour <= (3 * polishCount)){
            dna.Colour = 1;
        }
        
        if(milColour <= (100* polishCount ) && milColour > (3 * polishCount)){
            dna.Colour = 2;   
        }
        
        if(milColour <=(2000 * polishCount) && milColour > (100 * polishCount)){
            dna.Colour = 3;   
        }
        
        // 10015 = 0 + 4 = 4
        // 10016 = 1 + 5 = 5
        
        if(dna.Colour == 0){
            dna.Colour = randomValue % 15 + 4;
        }
    
        dna.CutShape = uint256(keccak256(abi.encode(randomValue, dna.Colour)))  % 14 + 1;
        dna.Clarity = uint256(keccak256(abi.encode(randomValue, dna.CutShape))) % 23 + 1;
        dna.CutQuality = uint256(keccak256(abi.encode(randomValue, dna.Clarity))) % 14 + 1;
        dna.Carat = uint256(keccak256(abi.encode(randomValue, dna.CutQuality))) % 10000 + 1;
        dna.PolishCount = polishCount;
        
        nextRnd = uint256(keccak256(abi.encode(randomValue, dna.Carat)));
        returnDna = dna ;
    }
}

library FeeSplitter{
    
    function distributeFunds(uint256 amount, address[] storage owners, address charityAddress, mapping(address => uint256) storage ownerBalances) internal {
        uint256 charityAmount = amount/2;
        uint256 toShare = amount - charityAmount;
        ownerBalances[charityAddress] += charityAmount;
 
        uint256 split = toShare / owners.length;
        uint256 remainder = toShare % owners.length;
        ownerBalances[charityAddress] += remainder;
        
        for(uint i=0; i < owners.length;i++) {
            ownerBalances[owners[i]] += split;
        }
     }
}
