// SPDX-License-Identifier: MIT
pragma solidity ^0.8.2;

import "@chainlink/contracts/src/v0.8/VRFConsumerBase.sol";
import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

interface IERC20 {
    function transfer(address _to, uint256 _amount) external returns (bool);
    function balanceOf(address account) external view returns (uint256);
}

contract GemGemsDiamond is VRFConsumerBase,ERC721, Pausable, Ownable {
    using Counters for Counters.Counter;
    
    event LogWithdrawn(address receiver, uint amount);
    event NFTMinted(uint256 indexed tokenId, uint256 Carat, uint256 Colour, uint256 Clarity, uint256 CutQuality,uint256 CutShape);
    
    Counters.Counter private _tokenIdCounter;
    bytes32 internal keyHash;
    
    mapping(uint256=>DnaGenerator.DiamondDna) public DiamondDna;
    mapping(address => uint256) public ownerBalances;
    mapping(address => uint256) private ownerMints;
    mapping(address => bool) private isFundRecipient;
    
    address[] private owners;
    address charityAddress;
    
    uint256 internal fee;
    uint256 public randomResult;
    
    uint256[] private polishCosts =[70000000000000000,25000000000000000,12500000000000000,12500000000000000,25000000000000000,750000000000000000];
    
    
            /*
            Colour
            shape,
            clarity,
            CutQuality
            carat
        
        */
    
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
    
    
    function _baseURI() internal pure override returns (string memory) {
        return "https://gemgems.io/diamonds/";
    }

    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }
    
    function withdrawLink() public onlyOwner{
        IERC20 tokenContract = IERC20(0xa36085F69e2889c224210F603D836748e7dC0088);
        
        tokenContract.transfer(msg.sender, tokenContract.balanceOf(address(this)));
    }
        
    /// @notice Withdraws the balance associated to the owner
    /// @dev deliberately not checking isOwner as you may have been removed but should still get your funds
    /// @dev setting balance to zero before send to prevent re-entry in case it is a contract address
    function withdraw() public { 
        require(ownerBalances[msg.sender] > 0);

        uint256 balanceToSend = ownerBalances[msg.sender];
        ownerBalances[msg.sender] = 0;

        (bool success, ) = msg.sender.call{value:balanceToSend}("");
        require(success, "Transfer failed.");

        emit LogWithdrawn(msg.sender, balanceToSend); 
    }
    
    function safeMint() public payable whenNotPaused {
      require(randomResult !=0, "no random yet");
      require(_tokenIdCounter.current() < 10000, "limit reached");
      require( msg.value >= 70000000000000000,"0.07 mint fee");
      require(ownerMints[msg.sender] < 20,"20 minted");
      
      ownerMints[msg.sender] = ownerMints[msg.sender] + 1;
      dnaMint(msg.sender,1,0);
    }
     
     function dnaMint(address to, uint256 polishCount, uint256 polishType) private {
        uint256 tokenId = _tokenIdCounter.current();
        _safeMint(to, tokenId);
        
        (DnaGenerator.DiamondDna memory dna, uint256 newRnd) = DnaGenerator.getDna(randomResult, polishCount, polishType);
        
        DiamondDna[tokenId] = dna;
        randomResult = newRnd;
        
        _tokenIdCounter.increment();
        
        emit NFTMinted(tokenId, dna.Carat, dna.Colour, dna.Clarity, dna.CutShape, dna.CutShape);
        FeeSplitter.distributeFunds(msg.value, owners ,charityAddress, ownerBalances);
    }
    
    function burnAndMint(uint256 tokenToBurn, uint256 polishType) public payable whenNotPaused {
        require(ERC721.ownerOf(tokenToBurn) == msg.sender,"Burn only yours");
        require(DiamondDna[tokenToBurn].PolishCount < 6,"5 polishes allowed");
        
        uint256 polishFee = polishCosts[polishType] * DiamondDna[tokenToBurn].PolishCount + 1;
        
        require(msg.value >= polishFee, string(abi.encodePacked(polishFee, " mint fee")));
        
        _burn(tokenToBurn);
        
        dnaMint(msg.sender, DiamondDna[tokenToBurn].PolishCount + 1, polishType);
        
        delete DiamondDna[tokenToBurn];
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
     
    function getDna(uint256 randomValue, uint256 polishCount, uint256 polishType) internal pure returns (DiamondDna memory returnDna, uint256 nextRnd) {
        
        DiamondDna memory dna;
        
        dna.RandomValue = randomValue;
        
        if(polishType == 0 || polishType == 1){
            uint256 milColour = randomValue % 1000000;
            if(milColour <= (100 * polishCount)){
                dna.Colour = 1;
            }
            else{
                if(milColour <= (1000 * polishCount )){
                  dna.Colour = 2;   
                }
                else{
                    if(milColour <=(2000 * polishCount)){
                        dna.Colour = 3;   
                    }
                    else{
                      dna.Colour = randomValue % 15 + 4;
                    }
                }
            }
        }
    
        if(polishType == 0 || polishType == 2){
        dna.CutShape = uint256(keccak256(abi.encode(randomValue, dna.Colour)))  % 14 + 1;    
        }
        
        if(polishType == 0 || polishType == 3){
            dna.Clarity = uint256(keccak256(abi.encode(randomValue, dna.CutShape))) % 23 + 1;
        }
        
        if(polishType == 0 || polishType == 4){
            dna.CutQuality = uint256(keccak256(abi.encode(randomValue, dna.Clarity))) % 6 + 1;
        }
        
        if(polishType == 0 || polishType == 5){
            dna.Carat = uint256(keccak256(abi.encode(randomValue, dna.CutQuality))) % 100000 + 1;
        }
        
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